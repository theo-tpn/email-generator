using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noctus.Domain.Entities;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.Application.Services
{
    public class LicenseKeyService : ILicenseKeyService
    {
        private readonly ILicenseKeyRepository _licenseKeyRepository;

        public LicenseKeyService(ILicenseKeyRepository licenseKeyRepository)
        {
            _licenseKeyRepository = licenseKeyRepository;
        }

        public bool IsLicenseKeyValid(LicenseKey key)
        {
            var lastFlag = key.KeyFlags.OrderByDescending(x => x.CreatedOn).FirstOrDefault();
            if (lastFlag == null)
                return false;
            if (lastFlag.Status == KeyStatus.Flagged || lastFlag.Status == KeyStatus.Revoked)
                return false;

            return true;
        }

        public LicenseKey RegisterKey(string licenseKey, IdentifiersInfo identifierInfo, string planId)
        {
            var key = new LicenseKey {Key = licenseKey, AccountsGenBuckets = new List<GenBucket>(){ new() {Ref = planId}}};
            key.KeyFlags.Add(new LicenseKeyFlag()
            {
                IdentifiersInfo = identifierInfo,
                Status = KeyStatus.Ok,
                Reason = KeyStatusReason.UsualRunning,
                Description = "Key Registration"
            });

            return key;
        }

        public void AddKeyEvent(LicenseKey key, LicenseKeyEvent lke)
        {
            key.KeyEvents.Add(lke);
            _licenseKeyRepository.CreateOrUpdate(key);
        }
    }
}
