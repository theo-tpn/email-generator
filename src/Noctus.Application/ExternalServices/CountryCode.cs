using System.Collections.Generic;
using Noctus.Domain.Models.Sms;

namespace Noctus.Application.ExternalServices
{
    public static class CountryCode
    {
        public static Dictionary<string, SmsCountryCode> SmsCountryCodeLookup = new Dictionary<string, SmsCountryCode>
        {
            {"FR", SmsCountryCode.FR},
            {"UK", SmsCountryCode.UK},
            {"RU", SmsCountryCode.RU},
            {"UA", SmsCountryCode.UA},
            {"CN", SmsCountryCode.CN},
            {"PL", SmsCountryCode.PL},
            {"IE", SmsCountryCode.IE},
            {"RS", SmsCountryCode.RS},
            {"RO", SmsCountryCode.RO},
            {"EE", SmsCountryCode.EE},
            {"DE", SmsCountryCode.DE},
            {"NL", SmsCountryCode.NL},
            {"AT", SmsCountryCode.AT},
            {"ES", SmsCountryCode.ES},
            {"CZ", SmsCountryCode.CZ},
            {"CY", SmsCountryCode.CY},
            {"BE", SmsCountryCode.BE},
            {"BG", SmsCountryCode.BG},
            {"HU", SmsCountryCode.HU},
            {"MDA", SmsCountryCode.MDA},
            {"IT", SmsCountryCode.IT},
            {"GE", SmsCountryCode.GE},
            {"GR", SmsCountryCode.GR},
            {"SK", SmsCountryCode.SK},
            {"AM", SmsCountryCode.AM},
            {"AL", SmsCountryCode.AL},
            {"DK", SmsCountryCode.DK},
            {"NO", SmsCountryCode.NO},
        };
    }
}