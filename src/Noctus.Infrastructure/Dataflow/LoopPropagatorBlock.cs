using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Noctus.Infrastructure.Dataflow
{
    public sealed class LoopPropagatorBlock<TInput, TOutput> : IReportIdle, IPropagatorBlock<TInput, TOutput>
    {
        public LoopPropagatorBlock(Func<TInput, TOutput> transform) : this(transform,
            new ExecutionDataflowBlockOptions())
        { }

        public LoopPropagatorBlock(Func<TInput, TOutput> transform, ExecutionDataflowBlockOptions options)
        {
            ExternalTransformSync = transform;
            InternalBlock = new TransformBlock<TInput, TOutput>((Func<TInput, TOutput>)InternalTransformSync, options);
        }

        public LoopPropagatorBlock(Func<TInput, Task<TOutput>> transform) : this(transform,
            new ExecutionDataflowBlockOptions())
        { }

        public LoopPropagatorBlock(Func<TInput, Task<TOutput>> transform, ExecutionDataflowBlockOptions options)
        {
            ExternalTransformAsync = transform;
            InternalBlock =
                new TransformBlock<TInput, TOutput>((Func<TInput, Task<TOutput>>)InternalTransformAsync, options);
        }

        public void Complete()
        {
            InternalBlock.Complete();
        }

        public Task Completion => InternalBlock.Completion;

        public bool IsIdle =>
            _itemsInProcess == 0 &&
            InternalBlock.InputCount == 0 &&
            InternalBlock.OutputCount == 0;

        private int _itemsInProcess;

        private TOutput InternalTransformSync(TInput input)
        {
            PreProcess();
            var externalResult = ExternalTransformSync.Invoke(input);
            PostProcess();
            return externalResult;
        }

        private async Task<TOutput> InternalTransformAsync(TInput input)
        {
            PreProcess();
            var externalResult = await ExternalTransformAsync.Invoke(input);
            PostProcess();
            return externalResult;
        }

        private void PreProcess()
        {
            Interlocked.Increment(ref _itemsInProcess);
        }

        private void PostProcess()
        {
            Interlocked.Decrement(ref _itemsInProcess);
        }

        private Func<TInput, TOutput> ExternalTransformSync { get; }

        private Func<TInput, Task<TOutput>> ExternalTransformAsync { get; }

        private TransformBlock<TInput, TOutput> InternalBlock { get; }

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            return InternalBlock.LinkTo(target, linkOptions);
        }

        void IDataflowBlock.Fault(Exception exception)
        {
            ((IDataflowBlock)InternalBlock).Fault(exception);
        }

        DataflowMessageStatus ITargetBlock<TInput>.OfferMessage(DataflowMessageHeader messageHeader,
            TInput messageValue, ISourceBlock<TInput> source, bool consumeToAccept)
        {
            return ((ITargetBlock<TInput>)InternalBlock).OfferMessage(messageHeader, messageValue, source,
                consumeToAccept);
        }

        TOutput ISourceBlock<TOutput>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target,
            out bool messageConsumed)
        {
            return ((ISourceBlock<TOutput>)InternalBlock).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        bool ISourceBlock<TOutput>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            return ((ISourceBlock<TOutput>)InternalBlock).ReserveMessage(messageHeader, target);
        }

        void ISourceBlock<TOutput>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            ((ISourceBlock<TOutput>)InternalBlock).ReleaseReservation(messageHeader, target);
        }
    }
}