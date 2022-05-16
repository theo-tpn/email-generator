using System.Collections.Generic;
using System.Net;
using FluentResults;
using Noctus.Domain.Models;

namespace Noctus.Domain.Interfaces.Repositories
{
    public interface IHarvestedCookiesRepository
    {
        Result<int> Insert(List<Cookie> cookies);
        bool Delete(int id);
        int Count();
        List<HarvestedCookies> Peek(int count);
    }
}