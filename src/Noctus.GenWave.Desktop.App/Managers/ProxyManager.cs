using ElectronNET.API;
using FluentResults;
using Noctus.Application.Services;
using Noctus.Domain;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Stl.Async;
using Stl.Fusion;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Noctus.GenWave.Desktop.App.Managers
{
    [ComputeService]
    public class ProxyManager
    {
        private readonly IProxyService _proxyService;
        public ProxyManager(IProxyService proxyService)
        {
            _proxyService = proxyService;
        }

        [ComputeMethod]
        public virtual async Task<IEnumerable<ProxiesSetEntity>> GetProxiesBucket()
        {
            return await _proxyService.GetBucket();
        }
        public Result CreateProxiesSet(string setName, string setContent)
        {
            var createResult = _proxyService.CreateSet(setName, setContent);
            using(Computed.Invalidate())
                GetProxiesBucket().Ignore();
            return createResult;
        }
        public Result DeleteProxiesSet(int setId)
        {
            var deleteResult = _proxyService.DeleteSet(setId);
            using (Computed.Invalidate())
                GetProxiesBucket().Ignore();
            return deleteResult;
        }
        public async Task<Result> RenameProxiesSet(int setId, string newName)
        {
            var renameResult = await _proxyService.RenameSet(setId, newName);
            using (Computed.Invalidate())
                GetProxiesBucket().Ignore();
            return renameResult;
        }
    }
}
