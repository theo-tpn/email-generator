using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Noctus.GenWave.Desktop.App.Managers.Background
{
    public class LicenseKeyCheckerManager : BackgroundService
    {
        private readonly AuthenticationManager _authenticationManager;

        public LicenseKeyCheckerManager(AuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _authenticationManager.RenewLogin();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
