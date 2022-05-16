using FluentResults;
using Noctus.Application.Helpers;
using Noctus.Domain;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Noctus.Infrastructure.Extensions;
using Stl.Async;
using Stl.Fusion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noctus.Application.Services
{
    [ComputeService(typeof(IProxyService))]
    public class ProxyService : IProxyService
    {
        private readonly IProxiesSetRepository _proxiesSetRepository;

        public ProxyService(IProxiesSetRepository proxiesSetRepository)
        {
            _proxiesSetRepository = proxiesSetRepository;
        }

        [ComputeMethod]
        public virtual Task<IEnumerable<ProxiesSetEntity>> GetBucket()
        {
            return _proxiesSetRepository.Get();
        }

        public Result CreateSet(string setName, string setContent)
        {
            try
            {
                var parsedProxies = ParseProxies(setContent);
                var insertResult = _proxiesSetRepository.Insert(setName, parsedProxies);
                if(insertResult.IsFailed) return Result.Fail(insertResult.Errors.First());
                using (Computed.Invalidate())
                    GetBucket().Ignore();
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
        public Result DeleteSet(int id)
        {
            var deleteResult = _proxiesSetRepository.Delete(id);
            using (Computed.Invalidate())
                GetBucket().Ignore();
            return !deleteResult.IsFailed ? Result.Ok() : Result.Fail("Cannot delete proxies set file.");
        }

        public async Task<Result<IEnumerable<Proxy>>> TakeProxies(int setId, int proxiesNumber)
        {
            var set = await _proxiesSetRepository.Find(setId);
            if (set == null) return Result.Fail("Cannot find given set");
            var proxies = set.Proxies.Take(proxiesNumber);
            foreach (var proxy in proxies)
            {
                set.Proxies.Remove(proxy);
            }
            _proxiesSetRepository.Update(set);
            return Result.Ok(proxies);
        }

        public async Task<Result> RenameSet(int setId, string newName)
        {
            var set = await _proxiesSetRepository.Find(setId);
            if (set == null) return Result.Fail("Cannot find given set");
            set.Name = newName;
            _proxiesSetRepository.Update(set);
            using (Computed.Invalidate())
                GetBucket().Ignore();
            return Result.Ok();
        }

        private List<Proxy> ParseProxies(string rawProxies)
        {
            var sr = new StringReader(rawProxies.Trim());
            string line;
            var proxies = new List<Proxy>();
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) continue;
                proxies.Add(ParseProxy(line));
            }

            return proxies;
        }
        private Proxy ParseProxy(string rawProxy)
        {
            var px = rawProxy.Split(':');
            return new Proxy
            {
                Ip = px[0],
                Port = px[1],
                Username = px[2],
                Password = px[3]
            };
        }
    }
}
