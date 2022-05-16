using FluentResults;
using LiteDB;
using Noctus.Domain;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Models;
using Stl.Fusion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Stl.Async;

namespace Noctus.Application.LocalPersistance
{
    [ComputeService(typeof(IProxiesSetRepository))]
    public class ProxiesSetRepository : IProxiesSetRepository
    {
        private readonly ILiteCollection<ProxiesSetEntity> _collection;

        public ProxiesSetRepository()
        {
            _collection = Database.Connection.GetCollection<ProxiesSetEntity>("proxies", BsonAutoId.Int32);
        }

        public Result<int> Insert(string name, IList<Proxy> proxies)
        {
            try
            {
                var date = DateTime.Now;
                var value = _collection.Insert(new ProxiesSetEntity() { CreateDate = date, Proxies = proxies, Name = name});
                return Result.Ok(value.AsInt32);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to create entry", e));
            }
        }

        public bool Update(ProxiesSetEntity set)
        {
            var result = _collection.Update(set);

            using (Computed.Invalidate())
            {
                Get().Ignore();
                Find(set.Id).Ignore();
            }

            return result;
        }


        public Result Delete(int id)
        {
            var result = _collection.Delete(id);

            using (Computed.Invalidate())
            {
                Get().Ignore();
                Find(id).Ignore();
            }

            return result ? Result.Ok() :Result.Fail("Cannot delete given proxies set");
        }

        [ComputeMethod(AutoInvalidateTime = 60, KeepAliveTime = 61)]
        public virtual async Task<IEnumerable<ProxiesSetEntity>> Get()
        {
            return await Task.FromResult(_collection.Find(Query.All("Name", Query.Ascending)));
        }

        [ComputeMethod(AutoInvalidateTime = 60, KeepAliveTime = 61)]
        public virtual async Task<ProxiesSetEntity> Find(int id)
        {
            return await Task.FromResult(_collection.FindById(id));
        }
    }
}
