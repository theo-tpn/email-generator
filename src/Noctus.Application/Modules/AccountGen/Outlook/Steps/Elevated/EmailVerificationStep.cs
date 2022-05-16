using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.ExternalServices;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class EmailVerificationStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Verify Email";

        private readonly IRecoveryEmailService _recoveryEmailService;

        public EmailVerificationStep(IRecoveryEmailService recoveryEmailService)
        {
            _recoveryEmailService = recoveryEmailService;
        }

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            if (ctx.HotValues.ContainsKey(OutlookConstants.Keys.IsElevatedLogged)) return Result.Ok();

            var request = await client.GetAsync(OutlookConstants.Website.ManageUrl2, cancellationToken).ConfigureAwait(false);

            var body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var flowToken = OutlookConstants.RegexPatterns.FlowToken.Match(body).Groups[1].Value;
            var altEmailE = OutlookConstants.RegexPatterns.MobileNumE.Match(body).Groups[1].Value;
            var urlPost = OutlookConstants.RegexPatterns.UrlPost.Match(body).Groups[1].Value;

            if (string.IsNullOrEmpty(flowToken) || string.IsNullOrEmpty(urlPost))
                return Result.Fail(new Error("flowToken, mobileNumE, urlPost not found").WithMetadata("content", body));

            await client.PostAsync(OutlookConstants.Website.OneTimeCodeUrl,
                new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("login", ctx.Profile.Username),
                    new KeyValuePair<string, string>("flowtoken", flowToken),
                    new KeyValuePair<string, string>("AltEmailE", altEmailE),
                    new KeyValuePair<string, string>("purpose", "eOTT_OneTimePassword"),
                    new KeyValuePair<string, string>("channel", "Email"),
                    new KeyValuePair<string, string>("UIMode", "11"),
                    new KeyValuePair<string, string>("ProofConfirmation", ctx.RecoveryEmail.Username)
                }), cancellationToken).ConfigureAwait(false);

            await Task.Delay(15000, cancellationToken).ConfigureAwait(false);
            
            //chercher le code dans le mail
            using var emailClient = new EmailClient(ctx.RecoveryEmail.Username, ctx.RecoveryEmail.Password, ctx.RecoveryEmail.Provider);
            await emailClient.Connect(cancellationToken).ConfigureAwait(false);
            var codeResult = await emailClient.GetOutlookSecurityCode(OutlookConstants.RegexPatterns.SevenDigitCode, ctx.Profile.Username,cancellationToken).ConfigureAwait(false);
            
            if(codeResult.IsFailed) 
                return Result.Fail(new Error("No code found"));

            request = await client.PostAsync(urlPost,
                new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("login", ctx.Profile.Username),
                    new KeyValuePair<string, string>("PPFT", flowToken),
                    new KeyValuePair<string, string>("GeneralVerify", "false"),
                    new KeyValuePair<string, string>("AddTD", "true"),
                    new KeyValuePair<string, string>("hideSmsInMfaProofs", "false"),
                    new KeyValuePair<string, string>("ProofConfirmation", ctx.RecoveryEmail.Username),
                    new KeyValuePair<string, string>("otc", codeResult.Value),
                    new KeyValuePair<string, string>("type", "18")
                }), cancellationToken);
            
            var result = await RequestHelper.PostGeneratedForm(client,
                await request.Content.ReadAsStringAsync(cancellationToken), cancellationToken).ConfigureAwait(false);

            if (result.IsFailed)
                return result;

            //release le recovery code
            _recoveryEmailService.ReleaseRecoveryEmail(ctx.RecoveryEmail);

            ctx.HotValues[OutlookConstants.Keys.IsElevatedLogged] = "true";
            return Result.Ok();
        }
    }
}