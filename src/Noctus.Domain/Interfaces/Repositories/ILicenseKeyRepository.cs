﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noctus.Domain.Entities;

namespace Noctus.Domain.Interfaces.Repositories
{
    public interface ILicenseKeyRepository : IBaseRepository<LicenseKey>
    {
        bool TryGetByKey(string key, out LicenseKey resKey);
    }
}
