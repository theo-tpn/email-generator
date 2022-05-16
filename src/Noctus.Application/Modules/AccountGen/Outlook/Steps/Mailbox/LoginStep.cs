using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.PipelineComponents;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Mailbox
{
    public class LoginStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description => "Login to mailbox";
        
        protected override async Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx,
            CancellationToken cancellationToken)
        {
            var loginResult = await RequestHelper.Login(client, OutlookConstants.Website.MailboxLoginQueryString,
                ctx.Profile.Username,
                ctx.Profile.Password, cancellationToken).ConfigureAwait(false);

            if (loginResult.IsFailed)
                return loginResult;

            var formResult = await RequestHelper.PostGeneratedForm(client, loginResult.Value, cancellationToken).ConfigureAwait(false);

            if (formResult.IsFailed)
                return formResult;

            await client.GetAsync(OutlookConstants.Website.MailboxUrl, cancellationToken).ConfigureAwait(false);

            return Result.Ok();
        }
    }
}
