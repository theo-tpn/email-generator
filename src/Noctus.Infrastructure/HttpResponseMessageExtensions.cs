using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Noctus.Infrastructure
{
    public static class HttpResponseMessageExtensions
    {
        public static CookieContainer ReadCookies(this HttpResponseMessage response)
        {
            var pageUri = response.RequestMessage?.RequestUri;

            var cookieContainer = new CookieContainer();
            if (!response.Headers.TryGetValues("set-cookie", out var cookies)) return cookieContainer;
            foreach (var c in cookies)
            {
                cookieContainer.SetCookies(pageUri ?? throw new InvalidOperationException(), c);
            }

            return cookieContainer;
        }
    }
}
