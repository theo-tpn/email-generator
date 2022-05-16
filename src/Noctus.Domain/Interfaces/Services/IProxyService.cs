using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Models;

namespace Noctus.Domain.Interfaces.Services
{
    public interface IProxyService
    {
        Task<IEnumerable<ProxiesSetEntity>> GetBucket();
        Result CreateSet(string setName, string setContent);
        Result DeleteSet(int id);
        Task<Result<IEnumerable<Proxy>>> TakeProxies(int setId, int proxiesNumber);
        Task<Result> RenameSet(int setId, string newName);
    }
}
