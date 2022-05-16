using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Models;

namespace Noctus.Domain.Interfaces.Repositories
{
    public interface IAccountSetRepository
    {
        Result<int> Insert(string name, IList<Account> accounts);
        Task<AccountSetEntity> Find(int id);
        Task<IEnumerable<AccountSetEntity>> Get();
        bool Delete(int id);
        bool Update(AccountSetEntity set);
    }
}