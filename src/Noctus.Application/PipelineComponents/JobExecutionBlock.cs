using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Noctus.Infrastructure;
using Noctus.Infrastructure.Dataflow;

namespace Noctus.Application.PipelineComponents
{
    public class JobExecutionBlock : IPropagatorBlock<IJob, IJob> 
    {
        private IPropagatorBlock<IJob, IJob> Block { get; set; }

        private JobExecutionBlockOptions Options { get; }

        public JobExecutionBlock(JobExecutionBlockOptions options)
        {
            Options = options ?? JobExecutionBlockOptions.Default;
            Initialize();
        }

        private void Initialize()
        {
            var priorityBuffer = new PriorityBufferBlock<IJob>();

            var target = new ActionBlock<IJob>(async job =>
            {
                await priorityBuffer.SendAsync(job, Priority.Low);
            });

            var loop = new LoopPropagatorBlock<IJob, IJob>(async job =>
                {
                    job.StartProcessing();

                    if (job.IsCancelled)
                        return null; // discard job

                    var result = await job.ExecuteBlocks();

                    if (result.IsFailed)
                    {
                        if (job.IsCancelled)
                            return null; // discard job

                        await priorityBuffer.SendAsync(job, job.GetPriority());
                        return null;
                    }

                    return job;
                },
                new ExecutionDataflowBlockOptions
                    {MaxDegreeOfParallelism = Options.Threads, BoundedCapacity = Options.Threads, EnsureOrdered = false});

            var source = new TransformBlock<IJob, IJob>(job =>
            {
                job.Finish();
                return job;
            }, new ExecutionDataflowBlockOptions { EnsureOrdered = false });

            priorityBuffer.LinkTo(loop);
            loop.LinkTo(source, job => job != null);
            loop.LinkTo(DataflowBlock.NullTarget<IJob>());

            target.Completion.ContinueWith(async _ =>
            {
                try
                {
                    if (Options.EnableBlockTimeOut)
                    {
                        await loop.CompleteLoop(TimeSpan.FromMinutes(Options.BlockTimeOut), priorityBuffer);
                    }
                    else
                    {
                        await loop.CompleteLoop(priorityBuffer);
                    }

                    source.Complete();
                }
                catch (TimeoutException ex)
                {
                    ((IDataflowBlock)source).Fault(ex);
                }
            });

            Block = DataflowBlock.Encapsulate(target, source);
        }

        #region IPropagatorBlock

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IJob messageValue, ISourceBlock<IJob> source,
            bool consumeToAccept)
        {
            return Block.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Complete()
        {
            Block.Complete();
        }

        public void Fault(Exception exception)
        {
            Block.Fault(exception);
        }

        public Task Completion => Block.Completion;

        public IJob ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IJob> target, out bool messageConsumed)
        {
            return Block.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<IJob> target, DataflowLinkOptions linkOptions)
        {
            return Block.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IJob> target)
        {
            Block.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IJob> target)
        {
            return Block.ReserveMessage(messageHeader, target);
        }

        #endregion
    }
}