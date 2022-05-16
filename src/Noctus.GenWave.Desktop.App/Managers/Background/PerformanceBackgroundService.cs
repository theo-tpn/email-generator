using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Noctus.GenWave.Desktop.App.Managers.Background
{
    public class PerformanceBackgroundService : BackgroundService
    {
        private readonly PerformanceRecorder _performanceRecorder;

        private readonly PerformanceCounter _cpuGlobalCounter;
        private readonly PerformanceCounter _ramCounter;
        
        public PerformanceBackgroundService(PerformanceRecorder performanceRecorder)
        {
            _performanceRecorder = performanceRecorder;
            
            _cpuGlobalCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var cpuGlobalPercent = _cpuGlobalCounter?.NextValue() ?? 0;
                var ramPercent = _ramCounter?.NextValue() ?? 0;
                _performanceRecorder.PushData(cpuGlobalPercent, ramPercent);
                
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
