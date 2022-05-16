using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noctus.Domain.Models
{
    public class AccountGenBucket
    {
        public int CurrentStock { get; set; }
        public int MaximumStock { get; set; }
        public DateTime NextRefillAt { get; set; }
    }
}
