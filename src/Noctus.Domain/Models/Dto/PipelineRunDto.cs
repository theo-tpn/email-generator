using System.ComponentModel.DataAnnotations;

namespace Noctus.Domain.Models.Dto
{
    public class PipelineRunDto
    {
        [Required]
        public string LicenseKey { get; set; }
        [Required]
        public bool HasForwarding { get; set; }
        [Required]
        public string AccountCountryCode { get; set; }
        [Required]
        public string PvaCountryCode { get; set; }
        [Required]
        public int Jobs { get; set; }
        [Required]
        public int Parallelism { get; set; }
    }
}
