using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Persistence.Contexts;

namespace Noctus.Persistence.Repositories
{
    public class GenBucketConfigRepository : BaseRepository<GenBucketConfig>, IGenBucketConfigRepository
    {
        public GenBucketConfigRepository(NoctusDbContext context) : base(context)
        {
        }

        public GenBucketConfig GetByRef(string metaRef)
        {
            return GetAll().SingleOrDefault(x => string.Equals(x.Ref, metaRef));
        }
    }
}
