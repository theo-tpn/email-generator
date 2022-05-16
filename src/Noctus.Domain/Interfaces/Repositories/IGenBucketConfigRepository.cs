using Noctus.Domain.Entities;

namespace Noctus.Domain.Interfaces.Repositories
{
    public interface IGenBucketConfigRepository : IBaseRepository<GenBucketConfig>
    {
        GenBucketConfig GetByRef(string metaRef);
    }
}
