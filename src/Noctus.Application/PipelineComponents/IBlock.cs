using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Interfaces;

namespace Noctus.Application.PipelineComponents
{
    public interface IBlock<TContext> where TContext : IExecutionContext
    {
        BlockStatus Status { get; }
        BlockExecution BlockExecutionLog { get; }

        event EventHandler RaiseErrorEvent;

        Task<Result> Execute(TContext ctx, CancellationToken cancellationToken);
    }
}