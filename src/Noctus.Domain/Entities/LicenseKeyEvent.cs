using Microsoft.EntityFrameworkCore;

#nullable enable

namespace Noctus.Domain.Entities
{
    public class LicenseKeyEvent : BaseEntity
    {
        public KeyEvent Event { get; set; }

        private IdentifiersInfo _identifiersInfo;
        [BackingField(nameof(_identifiersInfo))]
        public virtual IdentifiersInfo IdentifiersInfo
        {
            get => _identifiersInfo;
            set => _identifiersInfo = value;
        }
    }

    public enum KeyEvent
    {
        Login = 0,
        Logout = 1,
        LoginFailed = 2,
        RenewLogin = 3,
    }
}
