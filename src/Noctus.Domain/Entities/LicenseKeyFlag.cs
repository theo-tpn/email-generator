#nullable enable
using Microsoft.EntityFrameworkCore;

namespace Noctus.Domain.Entities
{
    public class LicenseKeyFlag : BaseEntity
    {
        public KeyStatus Status { get; set; }
        public KeyStatusReason Reason { get; set; }
        public string Description { get; set; }

        private IdentifiersInfo _identifiersInfo;
        [BackingField(nameof(_identifiersInfo))]
        public virtual IdentifiersInfo IdentifiersInfo
        {
            get => _identifiersInfo;
            set => _identifiersInfo = value;
        }
    }

    public enum KeyStatus
    {
        Ok = 0,
        Supervision = 1,
        Flagged = 2,
        Revoked = 3
    }

    public enum KeyStatusReason
    {
        MaliciousSoftware = 0,
        SoldNotAllowed = 1,
        RentNotAllowed = 2,
        UseExploit = 3,
        KeySharing = 4,
        SuspiciousActivity = 5,
        AdminForced = 6,
        UsualRunning = 7
    }
}
