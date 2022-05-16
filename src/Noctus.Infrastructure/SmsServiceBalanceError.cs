using FluentResults;

namespace Noctus.Infrastructure
{
    public class SmsServiceBalanceError : Error
    {
        public SmsServiceBalanceError() : base("Sms service balance is empty")
        {
            
        }
    }
}
