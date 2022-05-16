using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using HtmlAgilityPack;

namespace Noctus.Application.Modules.AccountGen.Outlook
{
    public static class RequestHelper
    {
        public static async Task<Result<string>> Login(HttpClient client, string queryString, string username,
            string password,
            CancellationToken cancellationToken)
        {
            var request = await client.GetAsync($"https://login.live.com/login.srf?{queryString}", cancellationToken)
                .ConfigureAwait(false);

            var content = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var ppft = OutlookConstants.RegexPatterns.PPFT.Match(content).Groups[1].Value;
            var url = OutlookConstants.RegexPatterns.UrlPost.Match(content).Groups[1].Value;

            if (content.Contains("fmHF"))
                return Result.Ok(content);

            if (string.IsNullOrEmpty(ppft) || string.IsNullOrEmpty(url))
                return Result.Fail(new Error("PPFT, urlPost not found").WithMetadata("content", content));

            request = await client.PostAsync(url,
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("login", username),
                        new KeyValuePair<string, string>("passwd", password),
                        new KeyValuePair<string, string>("PPFT", ppft)
                    }), cancellationToken)
                .ConfigureAwait(false);

            content = await request.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return Result.Ok(content);
        }

        public static async Task<Result<string>> PostGeneratedForm(HttpClient client, string content,
            CancellationToken cancellationToken)
        {
            var htmlDocumentParser = new HtmlDocument();
            htmlDocumentParser.LoadHtml(content);

            var nodes = htmlDocumentParser.DocumentNode
                .SelectNodes("//input[@type='hidden']");

            if (nodes == null)
                return Result.Fail(new Error("Could not find form inputs").WithMetadata("content", content));

            var formValues =
                (from n in nodes
                    let id = n.GetAttributeValue("id", string.Empty)
                    let value = n.GetAttributeValue("value", string.Empty)
                    select new KeyValuePair<string, string>(id, value)).ToList();

            var url = htmlDocumentParser.DocumentNode
                .SelectSingleNode("//form")
                .GetAttributeValue("action", string.Empty);

            if (!formValues.Any() || string.IsNullOrEmpty(url))
                return Result.Fail(new Error("Failed to find expected values")
                    .WithMetadata("content", htmlDocumentParser.Text));

            var request = await client.PostAsync(url, new FormUrlEncodedContent(formValues), cancellationToken)
                .ConfigureAwait(false);

            return Result.Ok(await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
