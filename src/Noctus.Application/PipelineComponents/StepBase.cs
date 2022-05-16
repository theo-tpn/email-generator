using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Interfaces;
using Noctus.Domain.Interfaces.Pipeline;

namespace Noctus.Application.PipelineComponents
{
    public abstract class StepBase<TContext> : IStep where TContext : IExecutionContext
    {
        public long TimeTakenInMs { get; private set; }
        public abstract string Description { get; }
        
        public async Task<Result> Execute(HttpClient client, IExecutionContext ctx, CancellationToken cancellationToken)
        {
            var watch = new Stopwatch();

            watch.Start();

            try
            {
                return await ExecuteInner(client, (TContext)ctx, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
            finally
            {
                watch.Stop();
                TimeTakenInMs = watch.ElapsedMilliseconds;
            }
        }

        protected abstract Task<Result> ExecuteInner(HttpClient client, TContext ctx,
            CancellationToken cancellationToken);
    }
}
