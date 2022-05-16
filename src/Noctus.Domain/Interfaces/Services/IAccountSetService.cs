using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Noctus.Domain.Models;

namespace Noctus.Domain.Interfaces.Services
{
    public interface IAccountSetService
    {
        Task<int> GetGeneratedAccountsCount();
        Task RunVerification(int setId);
        Task NewsletterSubscription(int setId);
        Task<Result<int>> ImportAccounts(int setId, Dictionary<AccountColumnType, string> columns,
            IList<object> records);
        Task<Result<int>> ImportAccounts(Dictionary<AccountColumnType, string> columns,
            IList<object> records);
    }
}