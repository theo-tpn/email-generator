using Microsoft.EntityFrameworkCore;

namespace Noctus.Domain.Entities
{
    public class PipelineEvent : BaseEntity
    {
        public PipelineEventType EventType { get; set; }

        private IdentifiersInfo _identifiersInfo;
        [BackingField(nameof(_identifiersInfo))]
        public virtual IdentifiersInfo IdentifiersInfo
        {
            get => _identifiersInfo;
            set => _identifiersInfo = value;
        }
    }

    public enum PipelineEventType
    {
        Start = 0,
        Finish = 1
    }
}
