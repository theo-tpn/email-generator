using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noctus.Domain.Entities;

namespace Noctus.Domain.Interfaces.Services
{
    public interface ILicenseKeyService
    {
        bool IsLicenseKeyValid(LicenseKey key);
        LicenseKey RegisterKey(string licenseKey, IdentifiersInfo identifierInfo, string planId);
        void AddKeyEvent(LicenseKey key, LicenseKeyEvent lke);
    }
}
