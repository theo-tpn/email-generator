using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Noctus.Domain.Models.Emails;

namespace Noctus.Domain.Interfaces.Services
{
    public interface IRecoveryEmailService
    {
        Task<IEnumerable<RecoveryEmail>> GetAll();
        Task AddRecoveryEmail(string username, string password, EmailProvider provider);
        Task RemoveRecoveryEmail(int id);
        Task<RecoveryEmail> RequestRecoveryEmail();
        void ReleaseRecoveryEmail(RecoveryEmail email);
    }
}
