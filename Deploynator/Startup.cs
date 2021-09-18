using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using DevLab.AzureAdapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deploynator
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
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<EventBus>();
            services.AddSingleton<AudioStream>();
            services.AddSingleton<LcdScreen>();

            // services.AddSingleton<IAzureReleaseRepository, FakeAzureReleaseRepository>();
            services.AddSingleton<IAzureReleaseRepository, AzureReleaseRepository>();
            services.AddSingleton<DeploymentHandler>();

            services.AddHostedService<RaspberryBridge>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}