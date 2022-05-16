#nullable enable
using System;
using System.Collections.Generic;

namespace Noctus.Domain.Models
{
    public class MailProfile : ICloneable
    {
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Birthday { get; set; }
        public int Birthmonth { get; set; }
        public int Birthyear { get; set; }
        public string? CountryCode { get; set; }
        public string? PhoneCountryCode { get; set; }
        public string? Password { get; set; }
        public string? RecoveryCode { get; set; }
        public string? MasterForward { get; set; }
        public string? RecoveryEmail { get; set; }
        public MailProfileStatus Status { get; set; }
        public string? MainMail { get; set; }
        public List<MailProfile>? Aliases { get; set; }

        public MailProfile()
        {
            Status = MailProfileStatus.NOT_TESTED;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public enum MailProfileStatus
    {
        OK = 0,
        CLIPPED = 1,
        IN_PROCESS = 2,
        NOT_TESTED = 3
    }
}
