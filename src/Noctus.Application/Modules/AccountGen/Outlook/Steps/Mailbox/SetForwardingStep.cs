using System;
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
    public class SetForwardingStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Set forwarding";

        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(ctx.Profile.MasterForward)) 
                return Result.Ok();
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(OutlookConstants.Website.ForwardingUrl),
                Headers =
                {
                    {"x-owa-urlpostdata", BuildFwRulePostData(ctx.Profile.MasterForward)},
                    {"action", "NewInboxRule"},
                }
            };

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var json = JsonConvert.DeserializeObject<dynamic>(body);
            
            return json["WasSuccessful"] != true 
                ? Result.Fail("Cannot set forwarding") 
                : Result.Ok();
        }

        private static string BuildFwRulePostData(string master)
        {
            var sb = new StringBuilder();
            sb.Append(
                "{\"__type\":\"NewInboxRuleRequest:#Exchange\",\"Header\":{\"__type\":\"JsonRequestHeaders:#Exchange\",\"RequestServerVersion\":\"V2018_01_08\",\"TimeZoneContext\":{\"__type\":\"TimeZoneContext:#Exchange\",\"TimeZoneDefinition\":{\"__type\":\"TimeZoneDefinitionType:#Exchange\",\"Id\":\"Romance Standard Time\"}}},\"InboxRule\":{\"Name\":\"forwarding\",\"ForwardTo\":[{\"__type\":\"PeopleIdentity:#Exchange\",");
            sb.Append($"\"DisplayName\":\"{master}\",");
            sb.Append($"\"SmtpAddress\":\"{master}\",");
            sb.Append("\"RoutingType\":\"SMTP\"}],\"StopProcessingRules\":true}}");
            return sb.ToString();
        }
    }
}