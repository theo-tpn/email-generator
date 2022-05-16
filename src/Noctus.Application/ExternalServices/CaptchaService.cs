using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Options;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using TwoCaptcha;
using TwoCaptcha.Captcha;
using TwoCaptcha.Exceptions;
using TimeoutException = System.TimeoutException;

namespace Noctus.Application.ExternalServices
{
    public class CaptchaService : ICaptchaService
    {
        private TwoCaptcha.TwoCaptcha _solver;
        private string _twoCaptchaApiKey;

        public CaptchaService(IOptionsMonitor<UserSettings> userSettings)
        { 
            _twoCaptchaApiKey = userSettings.CurrentValue.ExternalServices.TwoCaptchaApiKey;
            CreateClient();
            
            userSettings.OnChange(settings =>
            {
                _twoCaptchaApiKey = settings.ExternalServices.TwoCaptchaApiKey;
                CreateClient();
            });
        }

        private void CreateClient()
        {
            _solver = new TwoCaptcha.TwoCaptcha(_twoCaptchaApiKey)
            {
                DefaultTimeout = 60 * 3
            };
        }

        public async Task<Result<string>> SolveFunCaptcha(string siteKey, string url, string userAgent)
        {
            var captcha = new FunCaptcha();
            captcha.SetSiteKey(siteKey);
            captcha.SetUrl(url);
            captcha.SetSUrl("https://api.arkoselabs.com");
            captcha.SetUserAgent(userAgent);

            try
            {
                await _solver.Solve(captcha);

                if (!string.IsNullOrEmpty(captcha.Code)) 
                    return Result.Ok(captcha.Code);

                await _solver.Report(captcha.Id, false);
                return Result.Fail(new Error("Captcha solving failed"));
            }
            catch (TimeoutException e)
            {
                return Result.Fail(new Error("Captcha solving timed out").WithMetadata("reason", e.Message));
            }
            catch (ApiException e)
            {
                return Result.Fail(new Error("Captcha solving failed").WithMetadata("reason", e.Message));
            }
        }

        public async Task<Result<double>> GetBalance()
        {
            try
            {
                var client = new ApiClient();
                
                var value = await client.Res(new Dictionary<string, string>
                {
                    {"key", _twoCaptchaApiKey},
                    {"action", "getbalance"}
                });

                return Result.Ok(double.Parse(value, CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                return Result.Fail(new Error(e.Message));
            }
        }

        public async Task<ApiKeyStatus> ApiKeyHealthCheck(string key)
        {
            try
            {
                var client = new ApiClient();

                var value = await client.Res(new Dictionary<string, string>
                {
                    {"key", key},
                    {"action", "getbalance"}
                });

                return ApiKeyStatus.Ok;
            }
            catch (Exception e)
            {
                return ApiKeyStatus.Ko;
            }
        }
    }
}
