using Microsoft.Extensions.Hosting;
using Noctus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Noctus.Domain.Interfaces.Services;

namespace Noctus.GenWave.Desktop.App.Managers.Background
{
    public class ProcessProtecterManager : BackgroundService
    {
        private readonly INoctusService _noctusService;
        private readonly AppManager _appManager;

        private readonly List<string> _forbiddenProcesses = new List<string>()
        {
            "ilspy",
            "fiddler",
            "dotpeek",
            "justdecompiler",
            "codereflect",
            "wireshark",
            "charles",
            "mitmproxy",
            "proxyman",
            "http toolkit",
            "smartsniff",
            "networkminer",
            "httprequester",
            "httpmaster"
        };

        public ProcessProtecterManager(INoctusService noctusService, AppManager appManager)
        {
            _noctusService = noctusService;
            _appManager = appManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var localProcesses = Process.GetProcesses();
                var res = localProcesses.Where(x => _forbiddenProcesses.Contains(x.ProcessName.ToLower().Trim())).ToList();
                if (res.Any())
                {
                    await _noctusService.AddLicenseKeyEventAsync(KeyStatus.Flagged,
                        KeyStatusReason.MaliciousSoftware, string.Join(", ", res.Select(x => x.ProcessName)));

                    _appManager.CloseApp();
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
