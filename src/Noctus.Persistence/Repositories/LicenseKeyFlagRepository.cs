using System.Linq;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Persistence.Contexts;

namespace Noctus.Persistence.Repositories
{
    public class LicenseKeyFlagRepository : BaseRepository<LicenseKeyFlag>, ILicenseKeyFlagRepository
    {
        public LicenseKeyFlagRepository(NoctusDbContext context) : base(context)
        {
        }

        public LicenseKeyFlag GetLatestFlag(string key)
        {
            return GetAll().OrderByDescending(x => x.CreatedOn).FirstOrDefault();
        }
    }
}
