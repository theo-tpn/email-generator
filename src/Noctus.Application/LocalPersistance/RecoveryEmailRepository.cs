using FluentResults;
using LiteDB;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Models.Emails;
using Stl.Async;
using Stl.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noctus.Application.LocalPersistance
{
    [ComputeService(typeof(IRecoveryEmailRepository))]
    public class RecoveryEmailRepository : IRecoveryEmailRepository
    {
        private readonly ILiteCollection<RecoveryEmail> _collection;

        public RecoveryEmailRepository()
        {
            _collection = Database.Connection.GetCollection<RecoveryEmail>("recovery_emails", BsonAutoId.Int32);
        }

        public Result<int> Insert(RecoveryEmail email)
        {
            try
            {
                email.Id = 0;
                email.CreateTime = DateTime.Now;
                var value = _collection.Insert(email);
                return Result.Ok(value.AsInt32);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to create entry", e));
            }
        }
        public bool Update(RecoveryEmail email)
        {
            var result = _collection.Update(email);

            using (Computed.Invalidate())
            {
                GetAll().Ignore();
                Find(email.Id).Ignore();
            }

            return result;
        }
        public bool Delete(int id)
        {
            var result = _collection.Delete(id);

            using (Computed.Invalidate())
            {
                GetAll().Ignore();
                Find(id).Ignore();
            }

            return result;
        }

        public List<RecoveryEmail> Peek(int count)
        {
            var items = _collection.Find(Query.All(), limit: count);
            _collection.DeleteMany(item => items.Select(i => i.Id).Contains(item.Id));
            return items.ToList();
        }

        [ComputeMethod(AutoInvalidateTime = 60, KeepAliveTime = 61)]
        public virtual async Task<IEnumerable<RecoveryEmail>> GetAll()
        {
            return await Task.FromResult(_collection.Find(Query.All("CreateDate", Query.Descending), limit: 50));
        }

        [ComputeMethod(AutoInvalidateTime = 60, KeepAliveTime = 61)]
        public virtual async Task<RecoveryEmail> Find(int id)
        {
            return await Task.FromResult(_collection.FindById(id));
        }
    }
}
