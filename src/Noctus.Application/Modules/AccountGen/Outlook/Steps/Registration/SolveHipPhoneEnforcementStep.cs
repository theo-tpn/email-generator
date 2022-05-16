using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FluentResults;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Sms;
using Noctus.Infrastructure;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration
{
    public class SolveHipPhoneEnforcementStep : StepBase<AccountGenExecutionContext>
    {
        private readonly ISmsService _smsService;

        public override string Description => "Solve hip phone enforcement";

        public SolveHipPhoneEnforcementStep(ISmsService smsService)
        {
            _smsService = smsService;
        }

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            var uriBuilder = new UriBuilder(ctx.HotValues[OutlookConstants.Keys.HipUrl]);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["id"] = ctx.HotValues[OutlookConstants.Keys.HipSId];
            query["type"] = "sms";
            uriBuilder.Query = query.ToString() ?? string.Empty;
            var url = uriBuilder.ToString();

            HttpResponseMessage response;
            do
            {
                await Task.Delay(2500, cancellationToken).ConfigureAwait(false);
                response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            } while (!response.IsSuccessStatusCode);

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var match = OutlookConstants.RegexPatterns.SmsHip.Match(responseString);

            if (!match.Success)
                return Result.Fail(new Error("Cannot find hipUrl").WithMetadata("content", responseString));

            var hipUrlBuilder = new UriBuilder(match.Groups[2].Value.DecodeUtf8());
            var queryHip = HttpUtility.ParseQueryString(hipUrlBuilder.Query);
            queryHip["pn"] = ctx.HotValues[OutlookConstants.Keys.PhoneNumber];
            queryHip["pc"] = ctx.HotValues[OutlookConstants.Keys.PhoneNumberCountryCode];
            hipUrlBuilder.Query = queryHip.ToString() ?? string.Empty;

            var requestHip = await client.GetAsync(hipUrlBuilder.ToString(), cancellationToken).ConfigureAwait(false);
            var requestHipContent = await requestHip.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!requestHipContent.Contains("getHipDataResponse()"))
            {
                //ispHIPLimitExceeded
                if (requestHipContent.Contains("5"))
                {
                    await _smsService.ChangeActivationStatus(ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode],
                        ActivationStatus.REPORT_AND_CANCEL).ConfigureAwait(false);
                    ctx.HotValues.TryRemove(OutlookConstants.Keys.PhoneNumber, out _);
                    ctx.HotValues.TryRemove(OutlookConstants.Keys.PhoneNumberCountryCode, out _);
                    ctx.HotValues.TryRemove(OutlookConstants.Keys.PhoneNumberLastFourDigits, out _);
                    ctx.HotValues.TryRemove(OutlookConstants.Keys.PhoneOperationCode, out _);
                }

                return Result.Fail("Cannot send Hip Sms").WithError(requestHipContent);
            }

            ctx.HotValues[OutlookConstants.Keys.HipId] = queryHip["hid"];

            await _smsService
                .ChangeActivationStatus(ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode],
                    ActivationStatus.ACTIVATE).ConfigureAwait(false);

            var getSmsResult = await _smsService
                .GetSmsCode(ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode], cancellationToken)
                .ConfigureAwait(false);

            if (getSmsResult.IsFailed)
                return getSmsResult;

            ctx.HotValues[OutlookConstants.Keys.HipSmsSolution] = getSmsResult.Value;
            return Result.Ok();
        }
    }
}