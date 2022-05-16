using FluentResults;
using LiteDB;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Models;
using Noctus.Domain.Models.Emails;
using Stl.Async;
using Stl.Fusion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Noctus.Application.LocalPersistance
{
    [ComputeService(typeof(IAccountSetRepository))]
    public class AccountSetRepository : IAccountSetRepository
    {
        private readonly ILiteCollection<AccountSetEntity> _collection;

        public AccountSetRepository()
        {
            _collection = Database.Connection.GetCollection<AccountSetEntity>("accounts", BsonAutoId.Int32);
        }

        public Result<int> Insert(string name, IList<Account> accounts)
        {
            try
            {
                var date = DateTime.Now;
                name ??= date.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                var value = _collection.Insert(new AccountSetEntity { Name = name, CreateDate = date, Accounts = accounts, Provider = EmailProvider.Outlook });
                return Result.Ok(value.AsInt32);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to create account set", e));
            }
        }
        public bool Update(AccountSetEntity set)
        {
            var result = _collection.Update(set);

            using (Computed.Invalidate())
            {
                Get().Ignore();
                Find(set.Id).Ignore();
            }

            return result;
        }
        public bool Delete(int id)
        {
            var result = _collection.Delete(id);

            using (Computed.Invalidate())
            {
                Get().Ignore();
                Find(id).Ignore();
            }

            return result;
        }

        [ComputeMethod(AutoInvalidateTime = 60, KeepAliveTime = 61)]
        public virtual async Task<IEnumerable<AccountSetEntity>> Get()
        {
            return await Task.FromResult(_collection.Find(Query.All("CreateDate", Query.Descending), limit: 50));
        }

        [ComputeMethod(AutoInvalidateTime = 60, KeepAliveTime = 61)]
        public virtual async Task<AccountSetEntity> Find(int id)
        {
            return await Task.FromResult(_collection.FindById(id));
        }
    }
}