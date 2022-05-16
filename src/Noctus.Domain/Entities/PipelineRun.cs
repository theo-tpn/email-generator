using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Noctus.Domain.Models;

namespace Noctus.Domain.Entities
{
    public class PipelineRun : BaseEntity
    {
        private List<PipelineEvent> _events;
        [BackingField(nameof(_events))]
        public virtual List<PipelineEvent> Events
        {
            get => _events ??= new List<PipelineEvent>();
            set => _events = value;
        }

        public string PvaCountryCode { get; set; }
        public string AccountCountryCode { get; set; }
        public int JobsNumber { get; set; }
        public int JobsParallelism { get; set; }
        public bool HasForwarding { get; set; }
    }
}
