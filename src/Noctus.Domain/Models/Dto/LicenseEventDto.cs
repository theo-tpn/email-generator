using Noctus.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Noctus.Domain.Models.Dto
{
    public class LicenseEventDto
    {
        [Required]
        public string LicenseKey { get; set; }
        [Required]
        public string MbSerialInfo { get; set; }
        [Required]
        public KeyEvent Event { get; set; }
        public string UserDiscordId { get; set; }
    }
}
