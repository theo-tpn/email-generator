using System;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Infrastructure;

namespace Noctus.Application.PipelineComponents
{
    public interface IJob
    {
        bool IsCancelled { get; }
        JobState State { get; }

        Task<Result> ExecuteBlocks();
        void Cancel();
        void StartProcessing();
        void Finish();
        Priority GetPriority();
        event EventHandler OnCancel;
    }
}