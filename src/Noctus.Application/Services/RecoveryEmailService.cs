using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Emails;
using Stl.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stl.Async;

namespace Noctus.Application.Services
{
    [ComputeService(typeof(IRecoveryEmailService))]
    public class RecoveryEmailService : IRecoveryEmailService
    {
        private const int _maxRetries = 30;

        private readonly IRecoveryEmailRepository _recoveryEmailRepository;
        public RecoveryEmailService(IRecoveryEmailRepository recoveryEmailRepository)
        {
            _recoveryEmailRepository = recoveryEmailRepository;
        }

        public virtual Task<IEnumerable<RecoveryEmail>> GetAll()
        {
            return _recoveryEmailRepository.GetAll();
        }
        public async Task AddRecoveryEmail(string username, string password, EmailProvider provider)
        {
            var accounts = await _recoveryEmailRepository.GetAll();
            if (accounts.Any(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase))) return;
            _recoveryEmailRepository.Insert(new RecoveryEmail()
            {
                Username = username,
                Password = password,
                Provider = provider
            });
            using(Computed.Invalidate())
            {
                GetAll().Ignore();
            }
        }
        public async Task RemoveRecoveryEmail(int id)
        {
            _recoveryEmailRepository.Delete(id); 
            using (Computed.Invalidate())
            {
                GetAll().Ignore();
            }
        }
        public async Task<RecoveryEmail> RequestRecoveryEmail()
        {
            var retry = 0;
            do
            {
                if (retry > 0)
                    await Task.Delay(3000);
                retry++;

                var account = _recoveryEmailRepository.Peek(1);
                if (account == null) continue;
                return account.First();

            } while (retry <= _maxRetries);
            using (Computed.Invalidate())
            {
                GetAll().Ignore();
            }
            return null;
        }
        public async void ReleaseRecoveryEmail(RecoveryEmail email)
        {
            var result = _recoveryEmailRepository.Insert(email);
            if (result == null) return;
            using (Computed.Invalidate())
            {
                GetAll().Ignore();
            }
        }
    }
}
