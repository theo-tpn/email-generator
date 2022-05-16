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
    public class LicenseKeyEventRepository : BaseRepository<LicenseKeyEvent>, ILicenseKeyEventRepository
    {
        public LicenseKeyEventRepository(NoctusDbContext context) : base(context)
        {
        }
    }
}
