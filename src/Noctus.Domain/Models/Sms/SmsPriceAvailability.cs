using System;

namespace Noctus.Domain.Models.Sms
{
    public class SmsPriceAvailability
    {
        public SmsServiceType Type { get; }
        public double Price { get; }
        public int AvailablePhones { get; }
        public DateTime RequestTime { get; }

        public SmsPriceAvailability(SmsServiceType type, double price, int availablePhones, DateTime requestTime)
        {
            Type = type;
            Price = price;
            AvailablePhones = availablePhones;
            RequestTime = requestTime;
        }
    }
}