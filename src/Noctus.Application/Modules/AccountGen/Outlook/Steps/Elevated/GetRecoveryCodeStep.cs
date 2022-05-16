using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Newtonsoft.Json;
using Noctus.Application.PipelineComponents;
using Noctus.Infrastructure;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class GetRecoveryCodeStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Get recovery code";

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            if(!string.IsNullOrEmpty(ctx.Profile.RecoveryCode))
                return Result.Ok();

            // Go to manage page
            var request = await client.GetAsync(OutlookConstants.Website.ManageUrl, cancellationToken).ConfigureAwait(false);
            var result = await RequestHelper.PostGeneratedForm(client,
                await request.Content.ReadAsStringAsync(cancellationToken), cancellationToken).ConfigureAwait(false);

            if (result.IsFailed)
                return result;
            
            var encryptedNetId = OutlookConstants.RegexPatterns.EncryptedNetId.Match(result.Value).Groups[1].Value;
            var canary = OutlookConstants.RegexPatterns.ManagePageCanary.Match(result.Value).Groups[1].Value;

            if (string.IsNullOrEmpty(encryptedNetId) || string.IsNullOrEmpty(canary))
                return Result.Fail(new Error("EncryptedNetId or canary not found")
                    .WithMetadata("content", result.Value));

            // Ask for recovery code

            var message = new HttpRequestMessage
            {
                RequestUri = new Uri(OutlookConstants.Website.GenerateRecoveryUrl),
                Method = HttpMethod.Post,
                Content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        encryptedNetId = encryptedNetId.DecodeEncodedNonAsciiCharacters(),
                        hpgid = 201030,
                        scid = 100109
                    }),
                    Encoding.UTF8, "application/x-www-form-urlencoded"),
                Headers = {{"canary", canary.DecodeEncodedNonAsciiCharacters()}}
            };

            var generateRequest = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            var response = await generateRequest.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var json = JsonConvert.DeserializeObject<dynamic>(response);

            if (json.error != null)
                return Result.Fail(new Error("Recovery code generation failed")
                    .WithMetadata("error", json.error));

            ctx.Profile.RecoveryCode = json.recoveryCode;
            return Result.Ok();
        }
    }
}
