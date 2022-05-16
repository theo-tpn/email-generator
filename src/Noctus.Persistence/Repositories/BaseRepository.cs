using Microsoft.EntityFrameworkCore;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Persistence.Contexts;
using System.Collections.Generic;
using System.Linq;

namespace Noctus.Persistence.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected NoctusDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public BaseRepository(NoctusDbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public IEnumerable<TEntity> GetAll()
        {
            return DbSet;
        }

        public TEntity Get(int id)
        {
            return DbSet.Single(x => x.Id == id);
        }

        public TEntity Create(TEntity item)
        {
            var res = DbSet.Add(item);
            return res.Entity;
        }

        public void CreateRange(IEnumerable<TEntity> items)
        {
            DbSet.AddRange(items);
        }

        public void Update(TEntity item)
        {
            DbSet.Update(item);
        }

        public void UpdateRange(IEnumerable<TEntity> items)
        {
            DbSet.UpdateRange(items);
        }

        public void CreateOrUpdate(TEntity item)
        {
            if (Exist(item.Id)) Update(item);
            else Create(item);
        }

        public void CreateOrUpdateRange(IEnumerable<TEntity> items)
        {
            items.FirstOrDefault(x =>
            {
                CreateOrUpdate(x);
                return false;
            });
        }

        public void Delete(int id)
        {
            Delete(Get(id));
        }

        public void Delete(TEntity item)
        {
            DbSet.Remove(item);
        }

        public bool Exist(int id)
        {
            return DbSet.Any(x => x.Id == id);
        }
    }
}
