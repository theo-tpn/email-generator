using System;
using System.Collections;
using System.Collections.Generic;

namespace Noctus.Domain.Models
{
    public class ProxiesSetEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Proxy> Proxies { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Proxy
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseLocalhost { get; set; }
        public string Status { get; set; }
    }
}
