using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.Modules.AccountGen;
using Noctus.Application.PipelineComponents;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Modules.Shared
{
    public class RegisterNewsletterBlock : IBlock<AccountGenExecutionContext>
    {
        private readonly INewsletterService _service;

        public BlockStatus Status { get; private set; } = BlockStatus.WAITING;

        public BlockExecution BlockExecutionLog { get; }

        public event EventHandler RaiseErrorEvent;

        protected virtual void OnRaiseErrorEvent()
        {
            RaiseErrorEvent?.Invoke(this, EventArgs.Empty);
        }

        public RegisterNewsletterBlock(INewsletterService service)
        {
            _service = service;

            BlockExecutionLog = new BlockExecution("Shared_RegisterNewsletters", "Registering to newsletters");
        }

        public async Task<Result> Execute(AccountGenExecutionContext ctx, CancellationToken cancellationToken)
        {
            Status = BlockStatus.PROCESSING;

            BlockExecutionLog.StartedAt = BlockExecutionLog.StartedAt == DateTime.MinValue
                ? DateTime.Now
                : BlockExecutionLog.StartedAt;

            await _service.SubscribeToAll(ctx.Profile.Username);

            Status = BlockStatus.SUCCEEDED;
            BlockExecutionLog.EndedAt = DateTime.Now;

            return Result.Ok();
        }
    }
}
