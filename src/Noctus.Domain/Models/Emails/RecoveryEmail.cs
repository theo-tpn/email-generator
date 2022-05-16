using System;
using Newtonsoft.Json;

namespace Noctus.Domain.Models.Emails
{
    public class RecoveryEmail
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public EmailProvider Provider { get; set; }
        public DateTime CreateTime { get; set; }
        [JsonIgnoreAttribute] public bool IsActive { get; set; } = false;
    }
}
