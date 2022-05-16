using System.Collections.Concurrent;
using System.Net;
using Noctus.Domain.Models;

namespace Noctus.Domain.Interfaces
{
    public interface IExecutionContext
    {
        ConcurrentDictionary<string, string> HotValues { get; set; }
    }

    public interface IHttpExecutionContext : IExecutionContext
    {
        Proxy ProxyInfos { get; set; }
        string UserAgent { get; set; }
        CookieContainer CookieContainer { get; set; }
    }
}