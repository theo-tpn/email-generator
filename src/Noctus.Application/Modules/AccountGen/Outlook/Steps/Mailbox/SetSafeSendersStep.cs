using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Newtonsoft.Json;
using Noctus.Application.PipelineComponents;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Mailbox
{
    public class SetSafeSendersStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Set safe senders";

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            using var sr = new StreamReader("resources/data/safe_senders.txt");
            var safeSenders = new List<string>();
            string line;

            while ((line = await sr.ReadLineAsync()) != null)
                safeSenders.Add(line);

            var cleanSafeSender = safeSenders.Distinct().ToList();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(OutlookConstants.Website.SafeSenderUrl),
                Headers =
                {
                    {"x-owa-urlpostdata", BuildSafeSendersPostData(cleanSafeSender)},
                    {"action", "SetMailboxJunkEmailConfiguration"},
                }
            };

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var json = JsonConvert.DeserializeObject<dynamic>(body);

            return json["WasSuccessful"] != true 
                ? Result.Fail("Cannot set safe senders") 
                : Result.Ok();
        }

        private static string BuildSafeSendersPostData(IList<string> safeSenders)
        {
            if (safeSenders == null || !safeSenders.Any())
                return string.Empty;

            var sb = new StringBuilder();

            sb.Append("{\"" +
                      "__type\":\"SetMailboxJunkEmailConfigurationRequest:#Exchange\"," +
                      "\"Header\":" +
                      "{\"__type\":\"JsonRequestHeaders:#Exchange\",\"RequestServerVersion\":\"V2018_01_08\",\"TimeZoneContext\":{\"__type\":\"TimeZoneContext:#Exchange\",\"TimeZoneDefinition\":{\"__type\":\"TimeZoneDefinitionType:#Exchange\",\"Id\":\"Romance Standard Time\"}}},\"Options\":" +
                      "{\"TrustedSendersAndDomains\":[");

            sb.Append(safeSenders.Select(s => $"\"{s}\"").Aggregate((x, y) => $"{x},{y}"));

            sb.Append(
                "],\"TrustedRecipientsAndDomains\":[],\"BlockedSendersAndDomains\":[],\"Enabled\":true,\"TrustedListsOnly\":false,\"ContactsTrusted\":false}}");

            return sb.ToString();
        }
    }
}
