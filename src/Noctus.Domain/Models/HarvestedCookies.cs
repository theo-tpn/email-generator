using System;
using System.Collections.Generic;
using System.Net;

namespace Noctus.Domain.Models
{
    public class HarvestedCookies
    {
        public int Id { get; set; }
        public string Viewport { get; set; }
        public bool Headless { get; set; } = true;
        public DateTime CreationDate { get; }
        public List<Cookie> Cookies { get; set; } = new();

        public HarvestedCookies()
        {
            CreationDate = DateTime.Now;
        }
    }
}
