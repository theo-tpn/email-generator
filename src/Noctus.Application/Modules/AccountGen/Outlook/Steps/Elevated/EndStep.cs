using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.PipelineComponents;

namespace Noctus.Application.Modules.AccountGen.Outlook.Steps.Elevated
{
    public class EndStep : StepBase<AccountGenExecutionContext>
    {
        public override string Description { get; }

        protected override Task<Result> ExecuteInner(HttpClient client, AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            ctx.HotValues.Remove(OutlookConstants.Keys.IsElevatedLogged, out _);
            return Task.FromResult(Result.Ok());
        }
    }
}
