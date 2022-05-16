using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Noctus.Infrastructure.Dataflow
{
    public static class DataflowExtensions
    {
        public static IPropagatorBlock<T, T[]> CreateBatchBlock<T>(int batchSize,
            int timeout, GroupingDataflowBlockOptions dataflowBlockOptions = null)
        {
            dataflowBlockOptions ??= new GroupingDataflowBlockOptions();
            var batchBlock = new BatchBlock<T>(batchSize, dataflowBlockOptions);
            var timer = new Timer(_ =>
            {
                batchBlock.TriggerBatch();
            });
            var transformBlock = new TransformBlock<T, T>((T value) =>
            {
                timer.Change(timeout, Timeout.Infinite);
                return value;
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = dataflowBlockOptions.BoundedCapacity,
                CancellationToken = dataflowBlockOptions.CancellationToken,
                EnsureOrdered = dataflowBlockOptions.EnsureOrdered,
                MaxMessagesPerTask = dataflowBlockOptions.MaxMessagesPerTask,
                NameFormat = dataflowBlockOptions.NameFormat,
                TaskScheduler = dataflowBlockOptions.TaskScheduler
            });
            transformBlock.LinkTo(batchBlock, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });
            return DataflowBlock.Encapsulate(transformBlock, batchBlock);
        }
    }
}
