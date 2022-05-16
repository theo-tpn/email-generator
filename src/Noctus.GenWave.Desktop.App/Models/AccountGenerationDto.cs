using System.ComponentModel.DataAnnotations;
using Noctus.Application;
using Noctus.Domain.Models.Sms;

namespace Noctus.Genwave.Desktop.App.Models
{
    public class AccountGenerationDto
    {
        [Required(ErrorMessage = "You must select a module")]
        public ModuleType Module { get; set; }

        [Required(ErrorMessage = "You must select a proxies list")]
        public int ProxiesSetId { get; set; }

        [Required(ErrorMessage = "Please set a batch size")]
        [Range(1, 100)]
        public int TasksAmount { get; set; } = 1;

        [Range(1, 4)]
        [Required(ErrorMessage = "Please set block parallelism")]
        public int Parallelism { get; set; } = 1;

        [Required(ErrorMessage = "Country Phone is required")]
        public SmsCountryCode PhoneCountryCode { get; set; }

        [Required(ErrorMessage = "Account country is required")]
        public SmsCountryCode AccountCountryCode { get; set; }

        public string MasterFw { get; set; }
        
        [RegularExpression(@"^[0-9a-zA-Z_\-. ]+$", ErrorMessage = "Input contains forbidden characters")]
        [MaxLength(50)]
        public string OutputName { get; set; }
        public bool EnableEmailRecoveryVerification { get; set; }
        public bool EnablePhoneNumberVerification { get; set; }
        public bool UseHarvestedCookies { get; set; }
        public bool RegisterToNewsletter { get; set; }

        public string CustomPassword { get; set; }
        
        [Range(5, 30)]
        public int JobTimeoutInMinutes { get; set; } = 10;
    }
}
