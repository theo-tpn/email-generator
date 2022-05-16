#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using FluentResults;
using Noctus.Application.PipelineComponents;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class AddAliasesStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Adding Aliases";

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            if (ctx.Profile.Aliases == null) return Result.Ok();
            
            foreach (var alias in ctx.Profile.Aliases)
            {
                var request = await client.GetAsync(OutlookConstants.Website.SetAliasUrl, cancellationToken).ConfigureAwait(false);
                var requestContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                var htmlDocumentParser = new HtmlDocument();
                htmlDocumentParser.LoadHtml(requestContent);

                var nodes = htmlDocumentParser.DocumentNode
                    .SelectNodes("//input[@type='hidden']");

                var formValues =
                    (from n in nodes
                        let id = n.GetAttributeValue("name", string.Empty)
                        let value = n.GetAttributeValue("value", string.Empty)
                        select new KeyValuePair<string, string>(id, value)).ToList();

                var postAliasRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(OutlookConstants.Website.SetAliasUrl),
                    Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string?, string?>("canary",
                            formValues.SingleOrDefault(x => x.Key == "canary").Value),
                        new KeyValuePair<string?, string?>("DomainList", "outlook.com"),
                        new KeyValuePair<string?, string?>("AssociatedIdLive", alias.Split('@')[0]),
                        new KeyValuePair<string?, string?>("PostOption",
                            formValues.SingleOrDefault(x => x.Key == "PostOption").Value),
                        new KeyValuePair<string?, string?>("AddAssocIdOptions", "LIVE"),
                        new KeyValuePair<string?, string?>("UpSell",
                            formValues.SingleOrDefault(x => x.Key == "Upsell").Value),
                        new KeyValuePair<string?, string?>("SingleDomain",
                            formValues.SingleOrDefault(x => x.Key == "SingleDomain").Value),
                    })
                };
                
               await client.SendAsync(postAliasRequest, cancellationToken).ConfigureAwait(false);
            }

            return Result.Ok();
        }
    }
}
