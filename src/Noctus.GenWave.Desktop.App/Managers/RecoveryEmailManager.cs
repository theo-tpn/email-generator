using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models.Emails;
using Stl.Async;
using Stl.Fusion;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Noctus.GenWave.Desktop.App.Managers
{
    [ComputeService()]
    public class RecoveryEmailManager
    {
        private readonly IRecoveryEmailService _recoveryEmailService;

        public RecoveryEmailManager(IRecoveryEmailService recoveryEmailService)
        {
            _recoveryEmailService = recoveryEmailService;
        }

        [ComputeMethod]
        public virtual async Task<IEnumerable<RecoveryEmail>> GetRecoveryEmails()
        {
            return await _recoveryEmailService.GetAll();
        }

        public async Task AddRecoveryEmail(string username, string password, EmailProvider provider, bool applyDotTrick = false)
        {
            var usernames = new List<string>() {username};
            if (applyDotTrick)
            {
                usernames.AddRange(ApplyDotTrickedTo(username));
            }
            usernames.ForEach(x => _recoveryEmailService.AddRecoveryEmail(x, password, provider));

            using (Computed.Invalidate())
                GetRecoveryEmails().Ignore();
        }

        public async Task RemoveRecoveryEmail(RecoveryEmail email)
        {
            await _recoveryEmailService.RemoveRecoveryEmail(email.Id);
            using (Computed.Invalidate())
                GetRecoveryEmails().Ignore();
        }

        private IEnumerable<string> ApplyDotTrickedTo(string username)
        {
            var args = username.Split('@');
            var res = new List<string>();
            for(var i = 1; i < args[0].Length; i++)
            {
                if (args[0][i] == '.')
                {
                    i += 2;
                    continue;
                }
                res.Add($"{args[0].Insert(i, ".")}@{args[1]}");
            }

            return res;
        }
    }
}
