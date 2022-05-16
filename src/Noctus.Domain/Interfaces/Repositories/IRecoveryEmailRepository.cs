using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Models.Emails;

namespace Noctus.Domain.Interfaces.Repositories
{
    public interface IRecoveryEmailRepository
    {
        Result<int> Insert(RecoveryEmail email);
        bool Update(RecoveryEmail email);
        bool Delete(int id);
        List<RecoveryEmail> Peek(int count);
        Task<IEnumerable<RecoveryEmail>> GetAll();
        Task<RecoveryEmail> Find(int id);
    }
}
