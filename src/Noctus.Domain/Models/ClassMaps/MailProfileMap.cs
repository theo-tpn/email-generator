using CsvHelper.Configuration;

namespace Noctus.Domain.Models.ClassMaps
{
    public sealed class MailProfileMap : ClassMap<MailProfile>
    {
        public MailProfileMap()
        {
            Map(m => m.Username).Name("Username");
            Map(m => m.FirstName).Name("FirstName");
            Map(m => m.LastName).Name("LastName");
            Map(m => m.Birthday).Name("Birthday");
            Map(m => m.Birthmonth).Name("Birthmonth");
            Map(m => m.Birthyear).Name("Birthyear");
            Map(m => m.Password).Name("Password");
            Map(m => m.RecoveryCode).Name("RecoveryCode");

            Map(m => m.CountryCode).Name("AccountCountryCode").Optional();
            Map(m => m.MasterForward).Name("MasterForward").Optional();
            Map(m => m.PhoneCountryCode).Name("PhoneCountryCode").Optional();
            Map(m => m.MainMail).Name("MainMail").Optional();

            Map(m => m.Aliases).Ignore();
        }
    }
}
