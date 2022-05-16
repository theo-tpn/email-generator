using System;
using System.Collections.Concurrent;
using Noctus.Application.Modules.AccountGen;
using Noctus.Application.PipelineComponents;

namespace Noctus.Application
{
    public class PipelineRunInstance
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public ModuleType Type { get; set; }
        public AccountGenSettings Settings { get; set; }
        public RunStatus Status { get; set; } = RunStatus.QUEUED;
        public ConcurrentBag<Job<AccountGenExecutionContext>> Tasks { get; } =
            new ConcurrentBag<Job<AccountGenExecutionContext>>();
    }
}