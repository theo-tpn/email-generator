namespace Noctus.Domain.Models.Sms
{
    public class PhoneNumberOrder
    {
        public string PhoneCountryCode { get; }
        public string PhoneNumber { get; }

        public string OperationCode { get; }

        public PhoneNumberOrder(string phoneCc, string phoneNumber, string operationCode)
        {
            PhoneCountryCode = phoneCc;
            PhoneNumber = phoneNumber;
            OperationCode = operationCode;
        }
    }
}
