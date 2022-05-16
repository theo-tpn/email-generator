using System;
using System.Collections.Generic;
using FluentResults;

namespace Noctus.Application.PipelineComponents
{
    public class BlockExecution
    {
        public string Identifier { get; set; }
        public string BlockName { get; set; }
        public int RetryCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public List<StepExecution> StepExecution { get; }

        public BlockExecution(string id, string blockName)
        {
            Identifier = id;
            BlockName = blockName;
            RetryCount = 0;
            StartedAt = DateTime.MinValue;
            StepExecution = new List<StepExecution>();
        }
    }

    public class StepExecution
    {
        public string StepName { get; set; }
        public long TimeTakenMs { get; set; }
        public Result Result { get; set; }
    }
}