using Noctus.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;

namespace Noctus.Domain.Interfaces.Repositories
{
    public interface IProxiesSetRepository
    {
        Task<IEnumerable<ProxiesSetEntity>> Get();
        Task<ProxiesSetEntity> Find(int id);
        Result<int> Insert(string name, IList<Proxy> proxies);
        bool Update(ProxiesSetEntity set);
        Result Delete(int id);
    }
}
