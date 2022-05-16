using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.PipelineComponents;
using Noctus.Infrastructure;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration
{
    public class InitialStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Init";
        
        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            var result = await client.GetAsync(OutlookConstants.Website.SignUpUrl, cancellationToken).ConfigureAwait(false);
            var content = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var canaryMatch = Regex.Match(content, "(?:\"apiCanary\":\")(.*?)(?:\")");
            var fidMatch = Regex.Match(content, "(?:\"fid\":\")(.*?)(?:\")");
            var urlMatches = Regex.Matches(content, "(?:\"url\":\")(.*?)(?:\")");
            var hipUrl = urlMatches.SingleOrDefault(x => x.Value.Contains(OutlookConstants.Website.HipUrl));
            var uaidMatch = Regex.Match(content, "(?:\"uaid\":\")(.*?)(?:\")");

            var cipherPattern = OutlookConstants.RegexPatterns.RsaKey.Match(content);

            if (!canaryMatch.Success || !fidMatch.Success)
                return Result.Fail("Error");

            var cleanValue = canaryMatch.Value.Split(':').Last().Replace("\"", string.Empty).DecodeEncodedNonAsciiCharacters();
            client.DefaultRequestHeaders.Add("canary", cleanValue);

            var hipUrlRaw = hipUrl?.Value.Split('\"')[3];
            var uaidRaw = uaidMatch.Value.Split(':').Last().Replace("\"", string.Empty);

            ctx.HotValues[OutlookConstants.Keys.FId] = fidMatch.Value.Split(':').Last().Replace("\"", string.Empty);
            ctx.HotValues[OutlookConstants.Keys.HipUrl] = hipUrlRaw?.Replace("\"", string.Empty);
            ctx.HotValues[OutlookConstants.Keys.UaId] = uaidRaw;
            ctx.HotValues[OutlookConstants.Keys.EncryptKey] = cipherPattern.Groups[1].Value;
            ctx.HotValues[OutlookConstants.Keys.RandomNum] = cipherPattern.Groups[2].Value;

            return Result.Ok();
        }
    }
}
