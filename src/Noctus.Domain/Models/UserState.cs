using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noctus.Domain.Models
{
    public class UserState
    {
        public bool IsLogged { get; set; }
        public string LicenseKey { get; set; }
        public string DiscordId { get; set; }

        public UserState()
        {
            IsLogged = false;
        }
    }
}
