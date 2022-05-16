using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using Noctus.Application.Services;
using Noctus.Domain.Interfaces.Repositories;
using Noctus.Domain.Interfaces.Services;
using Noctus.Domain.Interfaces.UnitOfWork;
using Noctus.Persistence.Contexts;
using Noctus.Persistence.Repositories;
using Noctus.Persistence.UnitOfWork;

namespace Noctus.Api
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;

            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .AddJsonFile($"appsettings.{envName}.json",
                    optional: true,
                    reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<NoctusDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Noctus")).UseLazyLoadingProxies());

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<ILicenseKeyFlagRepository, LicenseKeyFlagRepository>();
            services.AddTransient<ILicenseKeyEventRepository, LicenseKeyEventRepository>();
            services.AddTransient<IPipelineRunRepository, PipelineRunRepository>();
            services.AddTransient<ILicenseKeyRepository, LicenseKeyRepository>();
            services.AddTransient<IGenBucketRepository, GenBucketRepository>();
            services.AddTransient<IGenBucketConfigRepository, GenBucketConfigRepository>();

            services.AddTransient<ILicenseKeyService, LicenseKeyService>();
            services.AddTransient<IGenBucketService, GenBucketService>();
            services.AddHttpClient<IMetalabsService, MetalabsService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Noctus.Api", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Noctus.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            //using (var scope = app.ApplicationServices.CreateScope())
            //using (var context = scope.ServiceProvider.GetService<NoctusDbContext>())
            //{
            //    context.Database.EnsureCreated();
            //    context?.Database.Migrate();
            //}


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
