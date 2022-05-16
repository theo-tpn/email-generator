namespace Noctus.Domain.Models
{
    public class UserSettings
    {
        public string GenwaveKey { get; set; }
        public string DiscordWebhook { get; set; }
        public AccountsGenerationSettings AccountsGeneration { get; set; }
        public ExternalServicesSettings ExternalServices { get; set; }

        public UserSettings()
        {
            ExternalServices = new ExternalServicesSettings();
            AccountsGeneration = new AccountsGenerationSettings();
        }
    }

    public class AccountsGenerationSettings
    {
        public int DefaultTasksNumber { get; set; }
        public int DefaultParallelTaskNumber { get; set; }
        public string DefaultMasterEmail { get; set; }
        public string DefaultPassword { get; set; }
    }
}
