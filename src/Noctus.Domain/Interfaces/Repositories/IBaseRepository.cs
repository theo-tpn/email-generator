using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noctus.Domain.Interfaces.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        IEnumerable<TEntity> GetAll();
        TEntity Get(int id);
        TEntity Create(TEntity item);
        void CreateRange(IEnumerable<TEntity> items);
        void Update(TEntity item);
        void UpdateRange(IEnumerable<TEntity> items);
        void CreateOrUpdate(TEntity item);
        void CreateOrUpdateRange(IEnumerable<TEntity> items);
        void Delete(int id);
        void Delete(TEntity item);
        bool Exist(int id);
    }
}
