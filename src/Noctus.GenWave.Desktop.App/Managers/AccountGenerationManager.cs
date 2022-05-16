using System;
using System.Collections.Concurrent;
using FluentResults;
using Noctus.Application.Modules.AccountGen;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Noctus.Domain.Models.Dto;
using Noctus.Genwave.Desktop.App.Models;
using Stl.Async;
using Stl.Fusion;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Noctus.Application;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Models.Sms;

namespace Noctus.GenWave.Desktop.App.Managers
{
    [ComputeService]
    public class AccountGenerationManager
    {
        private readonly IOptionsMonitor<UserSettings> _userSettings;
        private readonly INoctusService _noctusService;
        private readonly IOutlookAccountGenPipeline _outlookAccountGenPipeline;
        private readonly ExternalServiceManager _externalServiceManager;
        private readonly IHarvestedCookiesRepository _harvestedCookiesRepository;

        public AccountGenerationManager(
            IOptionsMonitor<UserSettings> userSettings,
            INoctusService noctusService, 
            IOutlookAccountGenPipeline outlookAccountGenPipeline, 
            ExternalServiceManager externalServiceManager,
            IHarvestedCookiesRepository harvestedCookiesRepository)
        {
            _userSettings = userSettings;
            _noctusService = noctusService;
            _outlookAccountGenPipeline = outlookAccountGenPipeline;
            _externalServiceManager = externalServiceManager;
            _harvestedCookiesRepository = harvestedCookiesRepository;
        }

        private readonly ConcurrentDictionary<int, PipelineRunInstance> _instances = new();

        [ComputeMethod(AutoInvalidateTime = 1)]
        public virtual async Task<List<PipelineRunInstance>> GetPipelineInstances()
        {
            return await Task.FromResult(_instances.Values.ToList());
        }

        [ComputeMethod(AutoInvalidateTime = 0.1)]
        public virtual async Task<PipelineRunInstance> GetPipelineInstance(int id)
        {
            return await Task.FromResult(_instances[id]);
        }

        [ComputeMethod(AutoInvalidateTime = 20)]
        public virtual async Task<AccountGenBucket> GetAccountGenBucket()
        {
            return await _noctusService.GetLicenseBucket(_userSettings.CurrentValue.GenwaveKey);
        }

        public async Task<Result<int>> CreateInstance(AccountGenerationDto dto)
        {
            if (_instances.Count(x => x.Value.Status == RunStatus.PROCESSING) >= 1)
                return Result.Fail(new Error("Max concurrent generation reached. Please retry later."));

            var bucket = await _noctusService.GetLicenseBucket(_userSettings.CurrentValue.GenwaveKey);
            if (bucket.CurrentStock < dto.TasksAmount)
                return Result.Fail(new Error("Insufficient bucket quantity. Wait or order a refill"));

            var settings = new AccountGenSettings
            {
                TasksAmount = dto.TasksAmount,
                Parallelism = dto.Parallelism,
                AccountCountryCode = dto.AccountCountryCode.ToString(),
                PhoneCountryCode = dto.PhoneCountryCode.ToString(),
                OutputFileName = dto.OutputName,
                MasterForwardMail = dto.MasterFw,
                ProxiesSetId = dto.ProxiesSetId,
                RegisterToNewsletter = dto.RegisterToNewsletter,
                Password = dto.CustomPassword,
                JobTimeoutInMinutes = dto.JobTimeoutInMinutes,
                EnablePhoneVerification = dto.EnablePhoneNumberVerification,
                EnableEmailRecoveryVerification = dto.EnableEmailRecoveryVerification,
                UseHarvestedCookies = dto.UseHarvestedCookies
            };

            IAccountGenPipeline gen;

            switch (dto.Module)
            {
                case ModuleType.Outlook:
                    gen = _outlookAccountGenPipeline;
                    break;
                default:
                    return Result.Fail($"No module named {dto.Module}");
            }

            var result = await _noctusService.CreatePipeline(new PipelineRunDto
            {
                LicenseKey = _userSettings.CurrentValue.GenwaveKey,
                HasForwarding = !string.IsNullOrEmpty(settings.MasterForwardMail),
                AccountCountryCode = settings.AccountCountryCode,
                PvaCountryCode = settings.PhoneCountryCode,
                Jobs = settings.TasksAmount,
                Parallelism = settings.Parallelism,
            });

            if (result.IsFailed)
                return Result.Fail("Failed to contact API");

            var instance = new PipelineRunInstance
            {
                Id = result.Value,
                Type = dto.Module,
                Settings = settings,
                CreateTime = DateTime.Now
            };

            _instances.TryAdd(result.Value, instance);

            using (Computed.Invalidate())
                GetPipelineInstance(result.Value).Ignore();

            return await gen.Run(instance);
        }

        public async Task<Result> CancelRemainingJobs(int id)
        {
            var instance = _instances[id];

            if (instance == null)
                return await Task.FromResult(Result.Fail(new Error("Generation instance not found")));

            foreach (var job in instance.Tasks.Where(x => x.State != JobState.FINISHED && !x.IsCancelled))
            {
                job.Cancel();
            }

            using (Computed.Invalidate())
                GetPipelineInstance(id).Ignore();

            return await Task.FromResult(Result.Ok());
        }

        [ComputeMethod(KeepAliveTime = 10)]
        public virtual async Task<ImmutableList<EstimatedPricesRecord>> Estimate(int tasks, bool useHarvestCookies,
            SmsCountryCode countryCode, bool enablePhoneNumberVerification, bool enableEmailRecoveryVerification)
        {
            var records = new List<EstimatedPricesRecord>();

            var (captchaCounter, smsCounter) = ComputeTasksPerService(tasks, useHarvestCookies,
                _harvestedCookiesRepository.Count());

            var balance2Captcha = await _externalServiceManager.GetBalance(ExternalService.TwoCaptcha);
            var captchaEstimatedCost = ComputeCaptchaCost(captchaCounter);
            records.Add(new EstimatedPricesRecord(ExternalService.TwoCaptcha, captchaEstimatedCost, balance2Captcha));

            var smsPrices = await _externalServiceManager.GetSmsLivePrice();
            var priceForCountry = smsPrices[countryCode].Price;

            var smsEstimatedCost = enablePhoneNumberVerification && !enableEmailRecoveryVerification
                ? ComputeSmsCost(captchaCounter + smsCounter, priceForCountry)
                : ComputeSmsCost(smsCounter, priceForCountry);
            var balanceSmsRu = await _externalServiceManager.GetBalance(ExternalService.SmsActivateRu);
            records.Add(new EstimatedPricesRecord(ExternalService.SmsActivateRu, smsEstimatedCost, balanceSmsRu));

            return records.ToImmutableList();
        }

        private (int captchaCounter, int smsCounter) ComputeTasksPerService(int tasks, bool useHarvestCookies, int availableCookies)
        {
            int captchaCounter;
            int smsCounter;

            if (useHarvestCookies)
            {            
                const double smsEnforcementRate = 0.1;

                var harvestTasks = availableCookies - tasks > 0
                    ? tasks
                    : availableCookies;

                captchaCounter = Convert.ToInt32(harvestTasks - harvestTasks * smsEnforcementRate);
                smsCounter = tasks - captchaCounter;
            }
            else
            {
                const double smsEnforcementRate = 0.8;

                captchaCounter = Convert.ToInt32(Math.Round(tasks - tasks * smsEnforcementRate));
                smsCounter = tasks - captchaCounter;
            }

            return (captchaCounter, smsCounter);
        }

        private double ComputeCaptchaCost(int tasks)
        {
            const double failRate = 0.1;
            var failCount = Math.Round(tasks * failRate);
            var estimatedCost = (tasks + failCount) * 0.00299; // rate is 2.99$ for 1K captcha
            return estimatedCost;
        }

        private double ComputeSmsCost(int tasks, double smsPrice)
        {
            const double failRate = 0.1;
            var failCount = Math.Round(tasks * failRate);
            var estimatedCost = (tasks + failCount) * smsPrice;
            return Math.Round(estimatedCost, 2);
        }
    }

    public record EstimatedPricesRecord(ExternalService ExternalService, double EstimatedPrice, double Balance);
}
