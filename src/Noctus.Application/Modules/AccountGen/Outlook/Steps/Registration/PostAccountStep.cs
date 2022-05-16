using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Jint;
using Noctus.Application.PipelineComponents;
using Noctus.Infrastructure;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration
{
    public class PostAccountStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Posting account";

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            var engine = new Engine().Execute(OutlookConstants.Website.MsEncryptionFunction);
            var cipherPass = engine.Invoke("Encrypt", "", "", "newpwd", ctx.Profile.Password,
                ctx.HotValues[OutlookConstants.Keys.EncryptKey], ctx.HotValues[OutlookConstants.Keys.RandomNum]).AsString();

            var rawContent = BuildBody(ctx, cipherPass);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(rawContent),
                RequestUri = new Uri(OutlookConstants.Website.CreateAccountUrl)
            };

            var result = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var stringResult = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            stringResult = stringResult.DecodeEncodedNonAsciiCharacters();
            stringResult = stringResult.Replace("\r\n", string.Empty);
            stringResult = stringResult.Replace(@"\", string.Empty);

            var challengeCodeMatch = Regex.Match(stringResult, "(?:\"code\":\")(.*?)(?:\")");
            var sidMatch = Regex.Match(stringResult, "(?:\"sid\":\")(.*?)(?:\")");

            ctx.HotValues[OutlookConstants.Keys.CipherPass] = cipherPass;
            ctx.HotValues[OutlookConstants.Keys.ChallengeType] = challengeCodeMatch.Value.Split(':').Last().Replace("\"", string.Empty);
            ctx.HotValues[OutlookConstants.Keys.HipSId] = sidMatch.Value.Split(':').Last().Replace("\"", string.Empty);

            return Result.Ok();
        }

        private string BuildBody(AccountGenExecutionContext ctx, string cipher) =>
            $"{{\"MemberName\":\"{ctx.Profile.Username}\",\"CheckAvailStateMap\":[\"{ctx.Profile.Username}:undefined\"],\"EvictionWarningShown\":[],\"UpgradeFlowToken\":{{}}," +
            $"\"FirstName\":\"{ctx.Profile.FirstName}\",\"LastName\":\"{ctx.Profile.LastName}\",\"MemberNameChangeCount\":2,\"MemberNameAvailableCount\":2,\"MemberNameUnavailableCount\":0,\"" +
            $"CipherValue\":\"{cipher}\"," +
            $"\"SKI\":\"{OutlookConstants.Website.PublicKey}\"," +
            $"\"BirthDate\":\"{ctx.Profile.Birthday}:{ctx.Profile.Birthmonth}:{ctx.Profile.Birthyear}\"," +
            $"\"Country\":\"{ctx.Profile.CountryCode}\"," +
            "\"IsOptOutEmailDefault\":true,\"IsOptOutEmailShown\":true,\"IsOptOutEmail\":true," +
            "\"LW\":true,\"SiteId\":\"68692\",\"IsRDM\":0,\"WReply\":null,\"ReturnUrl\":null,\"SignupReturnUrl\":null,\"uiflvr\":1001," +
            "\"SuggestedAccountType\":\"EASI\",\"SuggestionType\":\"Prefer\"," +
            "\"scid\":100118," +
            "\"hpgid\":200644," +
            $"\"HFId\":\"{ctx.HotValues[OutlookConstants.Keys.FId]}\"}}";
    }
}