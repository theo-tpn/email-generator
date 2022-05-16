using ElectronNET.API;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Noctus.GenWave.Desktop.App.Managers.Background
{
    public class AutoUpdateManager : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var keepLookingForUpdate = true;
            Electron.AutoUpdater.OnUpdateAvailable += info => { keepLookingForUpdate = false; };
            while (!stoppingToken.IsCancellationRequested && keepLookingForUpdate)
            {
                await Electron.AutoUpdater.CheckForUpdatesAndNotifyAsync();
                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }
    }
}
