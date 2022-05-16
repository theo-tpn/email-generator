namespace Noctus.Domain.Models.Sms
{
    public enum ActivationStatus
    {
        ACTIVATE = 1,
        REQUEST_OTHER_CODE = 3,
        COMPLETE = 6,
        REPORT_AND_CANCEL = 8
    }
}