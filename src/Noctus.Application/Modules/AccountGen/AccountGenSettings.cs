namespace Noctus.Application.Modules.AccountGen
{
    public class AccountGenSettings
    {
        public int TasksAmount { get; set; }
        public int Parallelism { get; set; }
        public string MasterForwardMail { get; set; }
        public string AccountCountryCode { get; set; }
        public string PhoneCountryCode { get; set; }
        public int ProxiesSetId { get; set; }
        public string OutputFileName { get; set; }
        public bool RegisterToNewsletter { get; set; }
        public string Password { get; set; }
        public int JobTimeoutInMinutes { get; set; }
        public bool EnableEmailRecoveryVerification { get; set; }
        public bool EnablePhoneVerification { get; set; }
        public bool UseHarvestedCookies { get; set; }
        public bool HasForwardingEnabled => !string.IsNullOrEmpty(MasterForwardMail);
    }
}