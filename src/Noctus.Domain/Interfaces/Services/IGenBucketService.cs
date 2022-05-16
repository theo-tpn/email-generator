using Noctus.Domain.Entities;

namespace Noctus.Domain.Interfaces.Services
{
    public interface IGenBucketService
    {
        bool DecreaseBucket(GenBucket bucket);
        bool FillBucket(GenBucket bucket);
        GenBucketConfig GetBucketConfig(GenBucket bucket);
    }
}
