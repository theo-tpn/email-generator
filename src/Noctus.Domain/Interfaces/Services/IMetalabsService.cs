using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noctus.Domain.Models;

namespace Noctus.Domain.Interfaces.Services
{
    public interface IMetalabsService
    {
        Task<MetalabsLicense> GetMetalabsLicense(string key);
    }
}
