using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Services
{
    public class GenBucketService : IGenBucketService
    {
        private readonly IGenBucketRepository _genBucketRepository;
        private readonly IGenBucketConfigRepository _genBucketConfigRepository;

        public GenBucketService(IGenBucketRepository genBucketRepository, IGenBucketConfigRepository genBucketConfigRepository)
        {
            _genBucketRepository = genBucketRepository;
            _genBucketConfigRepository = genBucketConfigRepository;
        }

        public bool DecreaseBucket(GenBucket bucket)
        {
            if (bucket.CurrentStock <= 0)
                return false;
            bucket.CurrentStock -= 1;
            _genBucketRepository.Update(bucket);
            return true;
        }

        public bool FillBucket(GenBucket bucket)
        {
            var genBucketConfig = GetBucketConfig(bucket);
            if (genBucketConfig == null) return false;
            bucket.CurrentStock = genBucketConfig.Quantity;
            _genBucketRepository.Update(bucket);
            return true;
        }

        public GenBucketConfig GetBucketConfig(GenBucket bucket)
        {
            return _genBucketConfigRepository.GetByRef(bucket.Ref);
        }
    }
}
