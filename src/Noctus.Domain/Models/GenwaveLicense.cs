using Noctus.Domain.Entities;

namespace Noctus.Domain.Models
{
    public class GenwaveLicense
    {
        public LicenseKeyFlag LicenseKeyFlags { get; set; }
        public MetalabsLicense MetalabsLicense { get; set; }
    }
}
