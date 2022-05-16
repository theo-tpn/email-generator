using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Persistence.Contexts;
using System.Linq;

namespace Noctus.Persistence.Repositories
{
    public class LicenseKeyRepository : BaseRepository<LicenseKey>, ILicenseKeyRepository
    {
        public LicenseKeyRepository(NoctusDbContext context) : base(context)
        {
        }

        public bool TryGetByKey(string key, out LicenseKey resKey)
        {
            resKey = GetAll().SingleOrDefault(x => string.Equals(x.Key, key));
            return resKey != null;
        }
    }
}
