using Noctus.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Noctus.Domain.Models.Dto
{
    public class LicenseFlagDto
    {
        [Required]
        public string LicenseKey { get; set; }
        [Required]
        public string MbSerialInfo { get; set; }
        [Required]
        public KeyStatus Flag { get; set; }
        [Required]
        public KeyStatusReason Reason { get; set; }
    }
}