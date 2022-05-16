using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Noctus.Application.ExternalServices;
using Noctus.Application.LocalPersistance;
using Noctus.Application.Modules.AccountGen;
using Noctus.Application.Modules.AccountGen.Outlook;
using Noctus.Application.Services;
using Noctus.Domain;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Models;
using Noctus.GenWave.Desktop.App.Managers;
using Noctus.GenWave.Desktop.App.Managers.Background;
using Serilog;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Extensions;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Noctus.GenWave.Desktop.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.File(Path.Combine(ResourcesHelper.DefaultLogsFolderPath, "log.txt"),
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMudServices(configuration =>
            {
                configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
                configuration.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            services.AddSignalR(options =>
            {
                // Setting a larger message size enable sending large size of data to server without seeing "Attempting to reconnect to the server"
                // This happened when user was pasting a large number of proxies into input
                //https://www.syncfusion.com/feedback/9846/attempting-to-reconnect-to-the-server-on-rowselected-rowdeselected-events
                options.MaximumReceiveMessageSize = 1000 * 1024;
            });

            services.AddFusion(fusion =>
            {
                fusion.AddLiveClock();

                fusion.AddComputeService<IProxyService, ProxyService>();
                fusion.AddComputeService<IAccountSetService, AccountSetService>();
                fusion.AddComputeService<IRecoveryEmailService, RecoveryEmailService>();

                fusion.AddComputeService<IAccountSetRepository, AccountSetRepository>();
                fusion.AddComputeService<IProxiesSetRepository, ProxiesSetRepository>();
                fusion.AddComputeService<IRecoveryEmailRepository, RecoveryEmailRepository>();
            });

            services.AddSingleton(c => new UpdateDelayer.Options
            {
                DelayDuration = TimeSpan.FromSeconds(0.05)
            });
            services.UseAttributeScanner().AddServicesFrom(Assembly.GetExecutingAssembly());

            services.Configure<UserSettings>(Configuration);

            services.AddSingleton<AppManager>();
            services.AddSingleton<ICaptchaService, CaptchaService>();

            services.AddTransient<IOutlookAccountGenPipeline, OutlookAccountGenPipeline>();

            services.AddHostedService<ProcessProtecterManager>();
            services.AddHostedService<LicenseKeyCheckerManager>();
            services.AddHostedService<AutoUpdateManager>();
            services.AddHostedService<PerformanceBackgroundService>();

            services.AddHttpClient<ISmsService, SmsService>();
            services.AddHttpClient<INoctusService, NoctusService>();
            services.AddHttpClient<INewsletterService, NewsletterService>(client =>
                client.Timeout = TimeSpan.FromSeconds(5));

            services.AddSingleton<IHarvestedCookiesRepository, HarvestedCookiesRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            ServicePointManager
                    .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            var appManager = app.ApplicationServices.GetService<AppManager>();
            appManager?.Initialize(env.IsDevelopment()).Wait();
        }
    }
}
