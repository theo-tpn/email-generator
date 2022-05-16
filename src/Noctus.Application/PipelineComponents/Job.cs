using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Application.Modules.AccountGen.Outlook;
using Noctus.Application.Modules.AccountGen.Outlook.Blocks;
using Noctus.Domain.Interfaces;
using Noctus.Infrastructure;

namespace Noctus.Application.PipelineComponents
{
    public class Job<TContext> : IJob where TContext : IExecutionContext
    {
        public Guid Id { get; set; }
        public JobState State { get; private set; }
        public TContext Context { get; set; }

        private readonly CancellationTokenSource _cts = new();
        private CancellationToken CancellationToken => _cts.Token;
        public bool IsCancelled => CancellationToken.IsCancellationRequested;

        private DateTime StartTime { get; set; } = DateTime.MinValue;
        private DateTime EndTime { get; set; } = DateTime.MinValue;

        public TimeSpan GetExecutionTime()
        {
            if (StartTime == DateTime.MinValue)
                return TimeSpan.Zero;

            return (EndTime != DateTime.MinValue ? EndTime : DateTime.Now) - StartTime;
        }

        public IReadOnlyList<BlockExecution> Logs => Blocks.Select(b => b.BlockExecutionLog).ToList();
        public List<IBlock<TContext>> Blocks = new();

        public event EventHandler OnCancel;

        private readonly TimeSpan _timeout;

        private Job()
        {
            Id = Guid.NewGuid();
            State = JobState.CREATED;
        }

        public Job(TContext ctx, TimeSpan timeout) : this()
        {
            Context = ctx;
            _timeout = timeout;
        }

        public void Cancel()
        {
            if(IsCancelled || State == JobState.FINISHED) return;
            
            EndTime = DateTime.Now;
            State = JobState.CANCELLED;
            OnCancel?.Invoke(this, null!);
            _cts.Cancel(true);
        }

        public void StartProcessing()
        {
            if (State == JobState.CREATED)
            {
                StartTime = DateTime.Now;
                State = JobState.PROCESSING;

                Task.Run(async () =>
                {
                    var timeoutTask = Task.Delay(_timeout, CancellationToken);

                    while (!IsCancelled || State == JobState.FINISHED)
                    {
                        if(timeoutTask.IsCompleted)
                            Cancel();

                        await Task.Delay(100, CancellationToken);
                    }
                }, CancellationToken).ConfigureAwait(false);
            }
        }

        public void Finish()
        {
            EndTime = DateTime.Now;
            State = JobState.FINISHED;
        }

        private IBlock<TContext> GetNextBlock() =>
            Blocks.FirstOrDefault(x => x.Status == BlockStatus.WAITING || x.Status == BlockStatus.FAILED);

        public IBlock<TContext> CurrentBlock => Blocks.FirstOrDefault(x => x.Status == BlockStatus.PROCESSING);

        public async Task<Result> ExecuteBlocks()
        {
            IBlock<TContext> block;

            while ((block = GetNextBlock()) != null)
            {
                block.RaiseErrorEvent += (_, _) => Cancel();

                var blockExecutionResult = await block.Execute(Context, CancellationToken).ConfigureAwait(false);

                if (!blockExecutionResult.IsFailed) continue;
                return blockExecutionResult;
            }

            return Result.Ok();
        }

        public Priority GetPriority()
        {
            var executionTime = DateTime.Now - StartTime;

            var lastExecutedBlock = Blocks.FirstOrDefault(x => x.Status == BlockStatus.FAILED);

            if (lastExecutedBlock is OutlookElevatedBlock)
                return Priority.High;

            if (Context.HotValues.ContainsKey(OutlookConstants.Keys.PhoneNumber) && executionTime.Minutes > 5)
                return Priority.High;

            if (Context.HotValues.ContainsKey(OutlookConstants.Keys.IsElevatedLogged))
                return Priority.High;

            if (executionTime.Minutes > 8)
                return Priority.High;

            return Priority.Medium;
        }
    }
}