using System;

namespace Noctus.Domain.Models
{
    public readonly struct ExternalService
    {
        public static ExternalService SmsActivateRu =
            new ("sms-ru", "₽", "https://sms-activate.ru/", "https://sms-activate.ru/en/buy",
                "https://sms-activate.ru/en");

        public static ExternalService TwoCaptcha =
            new ("2captcha", "$", "https://2captcha.com/", "https://2captcha.com/pay",
                "https://2captcha.com/statistics/realtime_monitor");

        private ExternalService(string label, string symbol, string website, string topUpUrl, string statsUrl)
        {
            Label = label;
            Symbol = symbol;
            Website = website;
            TopUpUrl = topUpUrl;
            StatsUrl = statsUrl;
        }

        public string Label { get; }
        public string Symbol { get; }
        public string Website { get; }
        public string TopUpUrl { get; }
        public string StatsUrl { get; }

        public bool Equals(ExternalService other)
        {
            return Label == other.Label && Symbol == other.Symbol && Website == other.Website;
        }

        public override bool Equals(object obj)
        {
            return obj is ExternalService other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Label, Symbol, Website);
        }

        public static bool operator ==(ExternalService x, ExternalService y) =>
            x.Label == y.Label;

        public static bool operator !=(ExternalService x, ExternalService y) =>
            x.Label != y.Label;
    }
}