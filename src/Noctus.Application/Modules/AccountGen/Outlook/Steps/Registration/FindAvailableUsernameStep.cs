using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Newtonsoft.Json;
using Noctus.Application.PipelineComponents;
using Noctus.Infrastructure;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Registration
{
    public class FindAvailableUsernameStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Find available username";
        
        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            dynamic json;
            string generatedUsername;

            do
            {
                generatedUsername =
                    $"{ctx.Profile.FirstName}{ctx.Profile.LastName}{RandomGenerator.Random.Next(0, 9999)}@outlook.com"
                        .ToLower().Trim();

                var bodyContent = new {signInName = generatedUsername};

                var httpMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri(OutlookConstants.Website.CheckAvailabilityUrl),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(bodyContent))
                };

                var response = await client.SendAsync(httpMessage, cancellationToken).ConfigureAwait(false);
                json = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
            } while (json?["isAvailable"] == false);

            ctx.Profile.Username = generatedUsername;
            //ctx.Profile.MainMail = generatedUsername;
            //ctx.Profile.Aliases?.ForEach(x => x.MainMail = generatedUsername);

            return Result.Ok();
        }
    }
}