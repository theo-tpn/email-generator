using System;

namespace Noctus.Application.PipelineComponents
{
    public class JobInfos
    {
        public JobInfos(Guid id, JobState state, string s)
        {
            Id = id;
            Message = s;
            ProducedAt = DateTime.Now;
            State = state;
        }

        public Guid Id { get; set; }
        public JobState State { get; set; }
        public string Message { get; set; }
        public DateTime ProducedAt { get; }
    }
}