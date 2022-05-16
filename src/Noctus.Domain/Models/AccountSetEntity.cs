using System;
using System.Collections.Generic;
using System.ComponentModel;
using Noctus.Domain.Models.Emails;

namespace Noctus.Domain.Models
{
    public class AccountSetEntity
    {
        public int Id { get; set; }
        public EmailProvider Provider { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public IList<Account> Accounts { get; set; } = new List<Account>();
        public AccountSetState CurrentState { get; set; } = AccountSetState.NONE;

        public bool IsInLockedState => CurrentState != AccountSetState.NONE;
        public void SetState(AccountSetState state) => CurrentState = state;
    }

    public enum AccountSetState
    {
        NONE,

        [Description("Clip testing")]
        CLIP_TESTING,
        
        [Description("Subscription to newsletters")]
        NEWSLETTER_SUBSCRIPTION
    }

    public class Account
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Birthday { get; set; }
        public int Birthmonth { get; set; }
        public int Birthyear { get; set; }
        public string CountryCode { get; set; }
        public string PhoneCountryCode { get; set; }
        public string Password { get; set; }
        public string RecoveryCode { get; set; }
        public string MasterForward { get; set; }
        public string RecoveryEmail { get; set; }
        public IList<string> Aliases { get; set; } = new List<string>();
        public ClipStatus ClipStatus { get; set; } = ClipStatus.NOT_TESTED;
        public DateTime LastClipVerification { get; set; }
    }

    public enum ClipStatus
    {
        [Description("Not tested")]
        NOT_TESTED,

        [Description("Valid")]
        VALID,

        [Description("Clipped")]
        CLIPPED
    }

    public enum AccountColumnType
    {
        [Description("User name (mail)")]
        Username,

        [Description("Password")]
        Password,

        [Description("Recovery code")]
        RecoveryCode,

        [Description("Recovery mail")]
        RecoveryEmail,

        [Description("Forward mail")]
        MasterForwarding
    }

}
