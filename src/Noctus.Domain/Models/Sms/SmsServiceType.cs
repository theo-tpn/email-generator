namespace Noctus.Domain.Models.Sms
{
    public class SmsServiceType
    {
        private SmsServiceType(string label, string shortName, short includeRedirection)
        {
            Label = label;
            ShortName = shortName;
            IncludeRedirection = includeRedirection;
        }

        public string ShortName { get; }
        public string Label { get; }
        public short IncludeRedirection { get; }

        public static SmsServiceType Microsoft = new SmsServiceType("Microsoft", "mm", 0);
    }
}