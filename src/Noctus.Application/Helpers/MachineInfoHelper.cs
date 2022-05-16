using DeviceId;
using Noctus.Domain.Entities;

namespace Noctus.Application.Helpers
{
    public static class MachineInfoHelper
    {
        public static IdentifiersInfo BuildLicenseIdentifiersInfo()
        {
            return new()
            {
                MotherBoardSerialNumber = new DeviceIdBuilder().AddMotherboardSerialNumber().ToString(),
            };
        }
    }
}
