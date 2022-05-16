using FluentResults;
using HtmlAgilityPack;
using Jint;
using Noctus.Application.ExternalServices;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class LinkEmailToAccountStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Linking email";
        private readonly IRecoveryEmailService _recoveryEmailService;
        
        public LinkEmailToAccountStep(IRecoveryEmailService recoveryEmailService)
        {
            _recoveryEmailService = recoveryEmailService;
        }

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            if (ctx.HasRecoveryEmailLinked) 
                return Result.Ok();

            var loginResult = await RequestHelper.Login(client, OutlookConstants.Website.LoginManageAccountQueryString,
                ctx.Profile.Username,
                ctx.Profile.Password, cancellationToken).ConfigureAwait(false);

            if (loginResult.IsFailed)
                return loginResult;

            var formResult = await RequestHelper.PostGeneratedForm(client, loginResult.Value, cancellationToken).ConfigureAwait(false);

            if (formResult.IsFailed)
                return formResult;

            var canary = new Regex("name=\"canary\" value=\"(.*?)\"").Match(formResult.Value).Groups[1].Value;

            if (string.IsNullOrEmpty(canary))
                return Result.Fail(new Error("canary not found").WithMetadata("content", formResult.Value));

            if (ctx.RecoveryEmail == null)
            {
                var recoveryEmail = await _recoveryEmailService.RequestRecoveryEmail().ConfigureAwait(false);
                if (recoveryEmail == null) return Result.Fail("no recovery email available ...");
                ctx.RecoveryEmail = recoveryEmail;
                ctx.Profile.RecoveryEmail = recoveryEmail.Username;
            }

            var sendMailCodeRequest = await client.PostAsync(OutlookConstants.Website.AddProofUrl,
                new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("iProofOptions", "Email"),
                    new KeyValuePair<string, string>("DisplayPhoneCountryISO", ctx.Profile.PhoneCountryCode),
                    new KeyValuePair<string, string>("DisplayPhoneNumber", ""),
                    new KeyValuePair<string, string>("canary", canary),
                    new KeyValuePair<string, string>("action", "AddProof"),
                    new KeyValuePair<string, string>("PhoneNumber", ""),
                    new KeyValuePair<string, string>("PhoneCountryISO", ""),
                    new KeyValuePair<string, string>("EmailAddress", ctx.RecoveryEmail.Username)
                }), cancellationToken).ConfigureAwait(false);

            await Task.Delay(15000, cancellationToken).ConfigureAwait(false);

            var content = await sendMailCodeRequest.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var cipherPattern = OutlookConstants.RegexPatterns.RsaKey.Match(content);
            var key = cipherPattern.Groups[1].Value;
            var randomNum = cipherPattern.Groups[2].Value;

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(randomNum))
                return Result.Fail(new Error("key or randomNum empty").WithMetadata("content", content));

            var htmlDocumentParser = new HtmlDocument();
            htmlDocumentParser.LoadHtml(content);

            var iProofOptionsNode = htmlDocumentParser.GetElementbyId("iProof0");
            var iProofOptions = iProofOptionsNode.GetAttributeValue("value", string.Empty);
            var form = htmlDocumentParser.GetElementbyId("frmVerifyProof");
            var url = form.GetAttributeValue("action", string.Empty);

            //scan du mail de recup pour aller chercher le code
            using var emailClient = new EmailClient(ctx.RecoveryEmail.Username, ctx.RecoveryEmail.Password, ctx.RecoveryEmail.Provider);
            await emailClient.Connect(cancellationToken).ConfigureAwait(false);
            var codeResult = await emailClient.GetOutlookSecurityCode(OutlookConstants.RegexPatterns.FourDigitCode, ctx.Profile.Username, cancellationToken).ConfigureAwait(false);
            if (codeResult.IsFailed) return Result.Fail(new Error("No code found"));

            var jsEngine = new Engine().Execute(OutlookConstants.Website.MsEncryptionFunction);
            var iEncryptedOtt = jsEngine.Invoke("Encrypt", "", codeResult.Value, "saproof", "", key, randomNum).AsString();

            sendMailCodeRequest = await client.PostAsync(url,
                new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("iProofOptions", iProofOptions),
                    new KeyValuePair<string, string>("iOttText", codeResult.Value),
                    new KeyValuePair<string, string>("iEncryptedOtt", iEncryptedOtt),
                    new KeyValuePair<string, string>("iPublicKey", OutlookConstants.Website.PublicKey),
                    new KeyValuePair<string, string>("canary", canary),
                    new KeyValuePair<string, string>("action", "VerifyProof"),
                    new KeyValuePair<string, string>("GeneralVerify", "0")
                }), cancellationToken);

            var sendMailCodeRequestContent = await sendMailCodeRequest.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!sendMailCodeRequest.IsSuccessStatusCode || !sendMailCodeRequestContent.Contains("fmHF") || sendMailCodeRequestContent.Contains("temporary problem"))
                return Result.Fail(new Error("Adding Email Recovery Failed"));
            
            ctx.HasRecoveryEmailLinked = true;
            return Result.Ok();
        }
    }
}
