using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using HtmlAgilityPack;
using Jint;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Sms;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class LinkPhoneNumberToAccountStep : StepBase<AccountGenExecutionContext>
    {
        private readonly ISmsService _smsService;

        public override string Description => "Add phone number to account";
        
        public LinkPhoneNumberToAccountStep(ISmsService smsService)
        {
            _smsService = smsService;
        }

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            if(ctx.HotValues.ContainsKey(OutlookConstants.Keys.IsElevatedLogged)) return Result.Ok();

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
                return Result.Fail(new Error("canary not found").WithMetadata("content", loginResult.Value));

            var request = await client.PostAsync(OutlookConstants.Website.AddProofUrl,
                new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("iProofOptions", "Phone"),
                    new KeyValuePair<string, string>("DisplayPhoneCountryISO", ctx.Profile.PhoneCountryCode),
                    new KeyValuePair<string, string>("DisplayPhoneNumber", ctx.HotValues[OutlookConstants.Keys.PhoneNumber]),
                    new KeyValuePair<string, string>("canary", canary),
                    new KeyValuePair<string, string>("action", "AddProof"),
                    new KeyValuePair<string, string>("PhoneNumber", ctx.HotValues[OutlookConstants.Keys.PhoneNumber]),
                    new KeyValuePair<string, string>("PhoneCountryISO", ctx.Profile.PhoneCountryCode)
                }), cancellationToken).ConfigureAwait(false);

            var content = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

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

            var pnOperationCode = ctx.HotValues[OutlookConstants.Keys.PhoneOperationCode];
            await _smsService.ChangeActivationStatus(pnOperationCode, ActivationStatus.ACTIVATE).ConfigureAwait(false);
            var result = await _smsService.GetSmsCode(pnOperationCode, cancellationToken).ConfigureAwait(false);

            if (result.IsFailed)
                return result;

            var jsEngine = new Engine().Execute(OutlookConstants.Website.MsEncryptionFunction);
            var iEncryptedOtt = jsEngine.Invoke("Encrypt", "", result.Value, "saproof", "", key, randomNum).AsString();

            request = await client.PostAsync(url,
                new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("iProofOptions", iProofOptions),
                    new KeyValuePair<string, string>("iOttText", result.Value),
                    new KeyValuePair<string, string>("iEncryptedOtt", iEncryptedOtt),
                    new KeyValuePair<string, string>("iPublicKey", OutlookConstants.Website.PublicKey),
                    new KeyValuePair<string, string>("canary", canary),
                    new KeyValuePair<string, string>("action", "VerifyProof"),
                    new KeyValuePair<string, string>("PhoneNumber", ctx.HotValues[OutlookConstants.Keys.PhoneNumber]),
                    new KeyValuePair<string, string>("GeneralVerify", "0")
                }), cancellationToken).ConfigureAwait(false);

            if (!request.IsSuccessStatusCode)
                return Result.Fail(new Error("Add phone number failed"));

            ctx.HasPhoneNumberLinked = true;
            return Result.Ok();
        }
    }
}