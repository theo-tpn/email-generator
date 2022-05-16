using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentResults;
using LiteDB;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Models;

namespace Noctus.Application.LocalPersistance
{
    public class HarvestedCookiesRepository : IHarvestedCookiesRepository
    {
        private readonly ILiteCollection<HarvestedCookies> _collection;

        public HarvestedCookiesRepository()
        {
            _collection = Database.Connection.GetCollection<HarvestedCookies>("harvested_cookies", BsonAutoId.Int32);
        }

        public Result<int> Insert(List<Cookie> cookies)
        {
            try
            {
                var value = _collection.Insert(new HarvestedCookies {Cookies = cookies});
                return Result.Ok(value.AsInt32);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to create entry", e));
            }
        }

        public bool Delete(int id) => _collection.Delete(id);

        public int Count() => _collection.Count();

        public List<HarvestedCookies> Peek(int count)
        {
            var items = _collection.Find(Query.All(nameof(HarvestedCookies.CreationDate)), limit: count);
            _collection.DeleteMany(item => items.Select(i => i.Id).Contains(item.Id));
            return items.ToList();
        }
    }
}
