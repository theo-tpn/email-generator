using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Sms;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class PhoneVerificationStep : StepBase<AccountGenExecutionContext>
    {
        private readonly ISmsService _smsService;

        public override string Description => "Phone verification";
        
        public PhoneVerificationStep(ISmsService smsService)
        {
            _smsService = smsService;
        }

        /// <summary>
        /// If user is here with the captcha enforcement challenge type, it means he executed <see cref="LinkPhoneNumberToAccountStep"/> previously (= he has auth cookie)
        /// </summary>
        /// <param name="challengeType"></param>
        /// <returns></returns>
        public bool IsAlreadyConnected(string challengeType) => challengeType == OutlookConstants.Website.CaptchaEnforcement;

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            if (ctx.HotValues.ContainsKey(OutlookConstants.Keys.IsElevatedLogged)) return Result.Ok();

            HttpResponseMessage request;
            string body;

            if (IsAlreadyConnected(ctx.HotValues[OutlookConstants.Keys.ChallengeType]))
            {
                request =
                    await client.GetAsync(OutlookConstants.Website.ManageUrl2, cancellationToken).ConfigureAwait(false);
                body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var loginResult = await RequestHelper.Login(client,
                    OutlookConstants.Website.LoginManageAccountQueryString, ctx.Profile.Username,
                    ctx.Profile.Password, cancellationToken).ConfigureAwait(false);

                if (loginResult.IsFailed)
                    return loginResult;
                body = loginResult.Value;
            }

            var flowToken = OutlookConstants.RegexPatterns.FlowToken.Match(body).Groups[1].Value;
            var mobileNumE = OutlookConstants.RegexPatterns.MobileNumE.Match(body).Groups[1].Value;
            var urlPost = OutlookConstants.RegexPatterns.UrlPost.Match(body).Groups[1].Value;

            if (string.IsNullOrEmpty(flowToken) || string.IsNullOrEmpty(mobileNumE) || string.IsNullOrEmpty(urlPost))
                return Result.Fail(new Error("flowToken, mobileNumE, urlPost not found").WithMetadata("content", body));

            await client.PostAsync(OutlookConstants.Website.OneTimeCodeUrl,
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new("login", ctx.Profile.Username),
                    new("flowtoken", flowToken),
                    new("MobileNumE", mobileNumE),
                    new("purpose", "eOTT_OneTimePassword"),
                    new("channel", "SMS"),
                    new("UIMode", "11"),
                    new("ProofConfirmation", ctx.HotValues[OutlookConstants.Keys.PhoneNumberLastFourDigits])
                }), cancellationToken).ConfigureAwait(false);

            await _smsService.ChangeActivationStatus(ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode], ActivationStatus.REQUEST_OTHER_CODE).ConfigureAwait(false);
            await Task.Delay(15000, cancellationToken).ConfigureAwait(false);
            
            var getSmsResult = await _smsService.GetSmsCode(ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode], cancellationToken).ConfigureAwait(false);

            if (getSmsResult.IsFailed)
                return getSmsResult;

            request = await client.PostAsync(urlPost,
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new("login", ctx.Profile.Username),
                    new("PPFT", flowToken),
                    new("SentProofIDE", mobileNumE),
                    new("GeneralVerify", "false"),
                    new("AddTD", "true"),
                    new("hideSmsInMfaProofs", "false"),
                    new("ProofConfirmation", ctx.HotValues[OutlookConstants.Keys.PhoneNumberLastFourDigits]),
                    new("otc", getSmsResult.Value),
                    new("type", "18")
                }), cancellationToken).ConfigureAwait(false);

            var result = await RequestHelper.PostGeneratedForm(client,
                await request.Content.ReadAsStringAsync(cancellationToken), cancellationToken).ConfigureAwait(false);

            if (result.IsFailed)
                return result;

            await _smsService.ChangeActivationStatus(ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode], ActivationStatus.COMPLETE).ConfigureAwait(false);
            ctx.HotValues[OutlookConstants.Keys.IsElevatedLogged] = "true";
            return Result.Ok();
        }
    }
}
