using System;
using System.Collections.Generic;
using System.Reflection;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Services;
using System.Threading.Tasks;
using Noctus.Domain.Models;
using Noctus.Domain.Models.Sms;
using Stl.Fusion;

namespace Noctus.GenWave.Desktop.App.Managers
{
    [ComputeService]
    public class ExternalServiceManager
    {
        private readonly ISmsService _smsService;
        private readonly ICaptchaService _captchaService;

        public ExternalServiceManager(ISmsService smsService, ICaptchaService captchaService)
        {
            _smsService = smsService;
            _captchaService = captchaService;
        }

        [ComputeMethod(KeepAliveTime = 60)]
        public virtual async Task<ApiKeyStatus> ApiKeyHealthCheck(ExternalService service, string apiKey)
        {
            return service switch
            {
                var x when x == ExternalService.TwoCaptcha => await TwoCaptchaApiKeyHealthCheck(apiKey),
                var x when x == ExternalService.SmsActivateRu => await SmsActivateRuApiKeyHealthCheck(apiKey),
                _ => ApiKeyStatus.Ko
            };
        }

        [ComputeMethod]
        public virtual async Task<ApiKeyStatus> SmsActivateRuApiKeyHealthCheck(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey)) return ApiKeyStatus.Ko;
            return await _smsService.ApiKeyHealthCheck(apiKey);
        }

        [ComputeMethod]
        public virtual async Task<ApiKeyStatus> TwoCaptchaApiKeyHealthCheck(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey)) return ApiKeyStatus.Ko;
            return await _captchaService.ApiKeyHealthCheck(apiKey);
        }

        [ComputeMethod(AutoInvalidateTime = 61, KeepAliveTime = 60)]
        public virtual async Task<double> GetBalance(ExternalService service)
        {
            var result = service switch
            {
                var x when x == ExternalService.TwoCaptcha => await _captchaService.GetBalance(),
                var x when x == ExternalService.SmsActivateRu => await _smsService.GetBalance(),
                _ => throw new ArgumentOutOfRangeException(nameof(service))
            };

            if (result.IsFailed)
                return 0;

            return result.Value;
        }

        [ComputeMethod(AutoInvalidateTime = 61, KeepAliveTime = 60)]
        public virtual async Task<Dictionary<SmsCountryCode, SmsPriceAvailability>> GetSmsLivePrice()
        {
            var result = await _smsService.GetPrices(SmsServiceType.Microsoft);
            return result.IsFailed ? new Dictionary<SmsCountryCode, SmsPriceAvailability>() : result.Value;
        }
    }
}
