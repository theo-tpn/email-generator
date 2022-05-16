using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noctus.Domain.Models.Emails;

namespace Noctus.Application.Helpers
{
    public static class EmailHelper
    {
        public static Dictionary<EmailProvider, string> ProvidersHost = new Dictionary<EmailProvider, string>
        {
            {EmailProvider.Gmail, "imap.gmail.com"},
            {EmailProvider.Outlook, "outlook.office365.com"}
        };
    }
}