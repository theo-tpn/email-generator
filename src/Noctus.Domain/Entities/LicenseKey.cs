using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Noctus.Domain.Entities
{
    public class LicenseKey : BaseEntity
    {
        public string Key { get; set; }

        private List<GenBucket> _accountsGenBucket;
        [BackingField(nameof(_accountsGenBucket))]
        public virtual List<GenBucket> AccountsGenBuckets
        {
            get => _accountsGenBucket ??= new List<GenBucket>();
            set => _accountsGenBucket = value;
        }

        private List<LicenseKeyEvent> _keyEvents;
        [BackingField(nameof(_keyEvents))]
        public virtual List<LicenseKeyEvent> KeyEvents
        {
            get => _keyEvents ??= new List<LicenseKeyEvent>();
            set => _keyEvents = value;
        }

        private List<LicenseKeyFlag> _keyFlags;
        [BackingField(nameof(_keyFlags))]
        public virtual List<LicenseKeyFlag> KeyFlags
        {
            get => _keyFlags ??= new List<LicenseKeyFlag>();
            set => _keyFlags = value;
        }

        private List<PipelineRun> _pipelineRuns;
        [BackingField(nameof(_pipelineRuns))]
        public virtual List<PipelineRun> PipelineRuns
        {
            get => _pipelineRuns ??= new List<PipelineRun>();
            set => _pipelineRuns = value;
        }
    }
}
