using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.PipelineComponents;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration
{
    public class FinalizeAccountStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Finalize account creation";

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            if (!ctx.HotValues.TryGetValue(OutlookConstants.Keys.ChallengeType, out var challengeType))
            {
                return Result.Fail("Challenge type not found in context");
            }

            var body = challengeType == OutlookConstants.Website.CaptchaEnforcement ? BuildEnforcementBody(ctx) : BuildHipBody(ctx);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(body),
                RequestUri = new Uri(OutlookConstants.Website.CreateAccountUrl)
            };

            var result = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            
            return content.Contains("R3_BAY", StringComparison.CurrentCultureIgnoreCase) || content.Contains("encPuid", StringComparison.InvariantCultureIgnoreCase)
                ? Result.Ok()
                : Result.Fail(new Error("Registration failed").WithMetadata("content", content));
        }

        private string BuildEnforcementBody(AccountGenExecutionContext ctx) =>
            $"{{\"MemberName\":\"{ctx.Profile.Username}\",\"CheckAvailStateMap\":[\"{ctx.Profile.Username}:undefined\"],\"EvictionWarningShown\":[],\"UpgradeFlowToken\":{{}}," +
            $"\"FirstName\":\"{ctx.Profile.FirstName}\",\"LastName\":\"{ctx.Profile.LastName}\",\"MemberNameChangeCount\":2,\"MemberNameAvailableCount\":2,\"MemberNameUnavailableCount\":0,\"" +
            $"CipherValue\":\"{ctx.HotValues[OutlookConstants.Keys.CipherPass]}\"," +
            $"\"SKI\":\"{OutlookConstants.Website.PublicKey}\"," +
            $"\"BirthDate\":\"{ctx.Profile.Birthday}:{ctx.Profile.Birthmonth}:{ctx.Profile.Birthyear}\"," +
            $"\"Country\":\"{ctx.Profile.CountryCode}\"," +
            $"\"IsOptOutEmailDefault\":true,\"IsOptOutEmailShown\":true,\"IsOptOutEmail\":true," +
            $"\"LW\":true,\"SiteId\":\"68692\",\"IsRDM\":0,\"WReply\":null,\"ReturnUrl\":null,\"SignupReturnUrl\":null,\"uiflvr\":1001," +
            $"\"SuggestedAccountType\":\"EASI\",\"SuggestionType\":\"Prefer\"," +
            $"\"scid\":100118," +
            $"\"hpgid\":200644," +
            $"\"HPId\": \"{OutlookConstants.Website.CaptchaKey}\"," +
            $"\"HType\":\"enforcement\"," +
            $"\"HSol\":\"{ctx.HotValues[OutlookConstants.Keys.HipCaptchaSolution]}\"," +
            $"\"HFId\":\"{ctx.HotValues[OutlookConstants.Keys.FId]}\"}}";

        private string BuildHipBody(AccountGenExecutionContext ctx) =>
            $"{{\"MemberName\":\"{ctx.Profile.Username}\",\"CheckAvailStateMap\":[\"{ctx.Profile.Username}:undefined\"],\"EvictionWarningShown\":[],\"UpgradeFlowToken\":{{}}," +
            $"\"FirstName\":\"{ctx.Profile.FirstName}\",\"LastName\":\"{ctx.Profile.LastName}\",\"MemberNameChangeCount\":2,\"MemberNameAvailableCount\":2,\"MemberNameUnavailableCount\":0,\"" +
            $"CipherValue\":\"{ctx.HotValues[OutlookConstants.Keys.CipherPass]}\"," +
            $"\"SKI\":\"{OutlookConstants.Website.PublicKey}\"," +
            $"\"BirthDate\":\"{ctx.Profile.Birthday}:{ctx.Profile.Birthmonth}:{ctx.Profile.Birthyear}\"," +
            $"\"Country\":\"{ctx.Profile.CountryCode}\"," +
            $"\"IsOptOutEmailDefault\":true,\"IsOptOutEmailShown\":true,\"IsOptOutEmail\":true," +
            $"\"LW\":true,\"SiteId\":\"68692\",\"IsRDM\":0,\"WReply\":null,\"ReturnUrl\":null,\"SignupReturnUrl\":null,\"uiflvr\":1001," +
            $"\"SuggestedAccountType\":\"EASI\",\"SuggestionType\":\"Prefer\"," +
            $"\"scid\":100118," +
            $"\"hpgid\":200644," +
            $"\"HId\": \"{ctx.HotValues[OutlookConstants.Keys.HipId]}\"," +
            $"\"HType\":\"sms\"," +
            $"\"HSol\":\"{ctx.HotValues[OutlookConstants.Keys.HipSmsSolution]}\"," +
            $"\"HSId\":\"{ctx.HotValues[OutlookConstants.Keys.HipSId]}\"," +
            $"\"HFId\":\"{ctx.HotValues[OutlookConstants.Keys.FId]}\"}}";
    }
}