#nullable enable
namespace Noctus.Domain.Entities
{
    public class IdentifiersInfo : BaseEntity
    {
        public string MotherBoardSerialNumber { get; set; }
        public string UserDiscordId { get; set; }
        public string? IpAddress { get; set; }
    }
}
