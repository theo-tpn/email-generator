using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentResults;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Noctus.Infrastructure;
using Account = Noctus.Domain.Models.Account;

namespace Noctus.Application.Modules.AccountGen
{
    public interface IAccountGenPipeline
    {
        Task<Result<int>> Run(PipelineRunInstance pipelineRunInstance);
    }

    public interface IOutlookAccountGenPipeline : IAccountGenPipeline { }

    public abstract class AccountGenPipeline : IAccountGenPipeline
    {
        protected readonly ISmsService SmsService;
        protected readonly ICaptchaService CaptchaService;
        protected readonly INewsletterService NewsletterService;
        protected readonly IRecoveryEmailService RecoveryEmailService;

        private readonly INoctusService _noctusService;
        private readonly IProxyService _proxyService;
        private readonly IAccountSetRepository _repository;
        private readonly IHarvestedCookiesRepository _harvestedCookiesRepository;

        private readonly DataflowLinkOptions _dataflowLinkOptions = new() { PropagateCompletion = true };

        protected AccountGenPipeline(
            ISmsService smsService,
            ICaptchaService captchaService,
            INewsletterService newsletterService,
            IRecoveryEmailService recoveryEmailService,
            INoctusService noctusService,
            IProxyService proxyService, 
            IAccountSetRepository repository, 
            IHarvestedCookiesRepository harvestedCookiesRepository)
        {
            SmsService = smsService;
            CaptchaService = captchaService;
            NewsletterService = newsletterService;
            RecoveryEmailService = recoveryEmailService;

            _noctusService = noctusService;
            _proxyService = proxyService;
            _repository = repository;
            _harvestedCookiesRepository = harvestedCookiesRepository;
        }

        protected abstract List<IBlock<AccountGenExecutionContext>> DefineJobBlocks(
            AccountGenSettings accountGenSettings);

        public async Task<Result<int>> Run(PipelineRunInstance pipelineRunInstance)
        {
            var accounts = new ConcurrentBag<Account>();
            pipelineRunInstance.Status = RunStatus.PROCESSING;

            var jigBlock =
                new TransformManyBlock<(int accountNumber, string countryCode, string phoneCountrCode, string masterForwardMail, string password), Account>(
                    async t => await Jig(t.accountNumber, t.countryCode, t.phoneCountrCode, t.masterForwardMail, t.password));

            var proxyBlock =
                new TransformManyBlock<(int accountNumber, int proxiesSetId), Proxy>(
                    async t => await ParseProxies(t.accountNumber, t.proxiesSetId));

            var cookiesBlock = new TransformManyBlock<(int accountNumber, bool useHarvestedCookies), CookieContainer>(
                t => BuildCookieContainer(t.accountNumber, t.useHarvestedCookies));

            var joinBlock = new JoinBlock<Account, Proxy, CookieContainer>(new GroupingDataflowBlockOptions
            {
                Greedy = false,
                EnsureOrdered = false
            });

            var createJobBlock = new TransformBlock<Tuple<Account, Proxy, CookieContainer>, IJob>(tuple =>
            {
                var job = CreateJob(tuple.Item1, tuple.Item2, tuple.Item3, pipelineRunInstance.Settings);
                pipelineRunInstance.Tasks.Add(job);
                return job;
            });

            var block = new JobExecutionBlock(new JobExecutionBlockOptions
            {
                Threads = pipelineRunInstance.Settings.Parallelism,
                EnableBlockTimeOut = true,
                BlockTimeOut = 60
            });

            var outputBlock = new ActionBlock<IJob>(job =>
            {
                var j = job as Job<AccountGenExecutionContext>;
                accounts.Add(j?.Context.Profile);
                _noctusService.SendFinishJob(pipelineRunInstance.Id);
            });

            jigBlock.LinkTo(joinBlock.Target1, _dataflowLinkOptions);
            proxyBlock.LinkTo(joinBlock.Target2, _dataflowLinkOptions);
            cookiesBlock.LinkTo(joinBlock.Target3, _dataflowLinkOptions);
            joinBlock.LinkTo(createJobBlock, _dataflowLinkOptions);
            createJobBlock.LinkTo(block, _dataflowLinkOptions);
            block.LinkTo(outputBlock, _dataflowLinkOptions);

            jigBlock.Post((accountNumber: pipelineRunInstance.Settings.TasksAmount,
                countryCode: pipelineRunInstance.Settings.AccountCountryCode,
                pipelineRunInstance.Settings.PhoneCountryCode,
                masterForwardMail: pipelineRunInstance.Settings.MasterForwardMail,
                pipelineRunInstance.Settings.Password));
            jigBlock.Complete();

            proxyBlock.Post((accountNumber: pipelineRunInstance.Settings.TasksAmount, proxyList: pipelineRunInstance.Settings.ProxiesSetId));
            proxyBlock.Complete();

            cookiesBlock.Post((GenerationBatchSize: pipelineRunInstance.Settings.TasksAmount, pipelineRunInstance.Settings.UseHarvestedCookies));
            cookiesBlock.Complete();

            await outputBlock.Completion;

            _repository.Insert(pipelineRunInstance.Settings.OutputFileName, accounts.ToList());
            pipelineRunInstance.Status = RunStatus.FINISHED;

            await _noctusService.CreatePipelineEvent(pipelineRunInstance.Id, PipelineEventType.Finish);

            return Result.Ok(pipelineRunInstance.Id);
        }

        private static async Task<List<Account>> Jig(int accountNumber, string countryCode, string phoneCountryCode, string masterForwardMail, string password)
        {
            var firstnameList = new List<string>();
            var lastnameList = new List<string>();

            using var srFirstname = new StreamReader("./resources/data/fr/firstname.txt");
            using var srLastname = new StreamReader("./resources/data/fr/lastname.txt");

            string line;
            while ((line = await srFirstname.ReadLineAsync()) != null)
                firstnameList.Add(line);
            while ((line = await srLastname.ReadLineAsync()) != null)
                lastnameList.Add(line);

            var accounts = new List<Account>();

            for (var i = 0; i < accountNumber; i++)
            {
                var account = new Account
                {
                    FirstName = firstnameList[RandomGenerator.Random.Next(0, firstnameList.Count)],
                    LastName = lastnameList[RandomGenerator.Random.Next(0, lastnameList.Count)],
                    Birthday = RandomGenerator.Random.Next(1, 28),
                    Birthmonth = RandomGenerator.Random.Next(1, 13),
                    Birthyear = RandomGenerator.Random.Next(1960, 2000),
                    Password = string.IsNullOrEmpty(password) ? StringExtensions.RandomString(10) : password,
                    CountryCode = countryCode,
                    MasterForward = masterForwardMail,
                    PhoneCountryCode = phoneCountryCode,
                    Aliases = new List<string>()
                };

                while (account.Aliases.Count < 2)
                {
                    var firstName = firstnameList[RandomGenerator.Random.Next(0, firstnameList.Count)];
                    var lastName = lastnameList[RandomGenerator.Random.Next(0, lastnameList.Count)];
                    var username =
                        $"{firstName}{lastName}{RandomGenerator.Random.Next(1, 999)}@outlook.com";
                    account.Aliases.Add(username);
                }

                accounts.Add(account);
            }

            return accounts;
        }

        private async Task<IList<Proxy>> ParseProxies(int accountNumber, int setId)
        {
            var proxies = new List<Proxy>();

            if (setId != -1)
            {
                var requestedProxies = await _proxyService.TakeProxies(setId, accountNumber);
                if (requestedProxies.IsSuccess)
                    proxies.AddRange(requestedProxies.Value);
            }

            if (proxies.Count < accountNumber)
            {
                proxies.AddRange(Enumerable.Repeat(new Proxy { UseLocalhost = true }, accountNumber - proxies.Count));
            }

            return proxies.Shuffle();
        }

        private IEnumerable<CookieContainer> BuildCookieContainer(int accountNumber, bool useHarvestedCookies)
        {
            var cookiesContainers = new List<CookieContainer>();

            if (useHarvestedCookies)
            {
                var harvestedItems = _harvestedCookiesRepository.Peek(accountNumber);
                
                if (harvestedItems.Any())
                {
                    foreach (var item in harvestedItems)
                    {
                        var cookieContainer = new CookieContainer();
                        
                        foreach (var cookie in item.Cookies)
                        {
                            cookieContainer.Add(cookie);
                        }

                        cookiesContainers.Add(cookieContainer);
                    }
                }
            }

            if (cookiesContainers.Count < accountNumber)
            {
                cookiesContainers.AddRange(Enumerable.Range(1, accountNumber - cookiesContainers.Count).Select(_ => new CookieContainer()));
            }

            return cookiesContainers;
        }

        private Job<AccountGenExecutionContext> CreateJob(Account account, Proxy proxyInfo,
            CookieContainer cookieContainer, AccountGenSettings accountGenSettings)
        {
            var ctx = new AccountGenExecutionContext
            {
                ProxyInfos = proxyInfo,
                Profile = account,
                UserAgent = UserAgents.List[RandomGenerator.Random.Next(0, UserAgents.List.Count - 1)],
                CookieContainer = cookieContainer
            };

            var job = new Job<AccountGenExecutionContext>(ctx, TimeSpan.FromMinutes(accountGenSettings.JobTimeoutInMinutes))
            {
                Blocks = DefineJobBlocks(accountGenSettings)
            };

            job.OnCancel += (sender, args) =>
            {
                if (ctx.RecoveryEmail == null) return;

                RecoveryEmailService.ReleaseRecoveryEmail(ctx.RecoveryEmail);
                ctx.RecoveryEmail = null;
            };

            return job;
        }
    }
}