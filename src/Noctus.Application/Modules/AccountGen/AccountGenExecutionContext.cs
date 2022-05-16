using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using Noctus.Domain.Interfaces;
using Noctus.Domain.Models;
using Noctus.Domain.Models.Emails;

namespace Noctus.Application.Modules.AccountGen
{
    public class AccountGenExecutionContext : IHttpExecutionContext
    {
        //public MailProfile Profile { get; set; }
        public Account Profile { get; set; }
        public Proxy ProxyInfos { get; set; }
        public ConcurrentDictionary<string, string> HotValues { get; set; }
        public string UserAgent { get; set; }
        public CookieContainer CookieContainer { get; set; }
        public bool HasPhoneNumberLinked { get; set; }
        public bool HasRecoveryEmailLinked { get; set; }
        public RecoveryEmail RecoveryEmail { get; set; }

        public AccountGenExecutionContext()
        {
            HotValues = new ConcurrentDictionary<string, string>();
        }
    }
}