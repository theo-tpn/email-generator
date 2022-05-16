using System.Diagnostics;
using System.Linq;
using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Noctus.Domain;
using Serilog;

namespace Noctus.GenWave.Desktop.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (PriorProcess() != null)
            {
                return;
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    ResourcesHelper.EnsureDefaultResources();
                    builder.AddJsonFile(ResourcesHelper.DefaultUserSettingsFilePath, false, true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseElectron(args)
                        .UseStartup<Startup>();
                }).UseSerilog();

        public static Process PriorProcess()
        {
            var curr = Process.GetCurrentProcess();
            var procs = Process.GetProcessesByName(curr.ProcessName);
            return procs.FirstOrDefault(p => (p.Id != curr.Id) && (p.MainModule?.FileName == curr.MainModule?.FileName));
        }
    }
}
