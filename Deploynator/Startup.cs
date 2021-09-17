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
            Thread.Sleep(10000);

            var eventBus = new EventBus();
            services.AddSingleton(eventBus);
            var audioStream = new AudioStream(eventBus);
            services.AddSingleton(audioStream);
            services.AddSingleton(new LcdScreen(eventBus));

            var httpClient = CreateHttpClient();

            services.AddSingleton(new DeploymentHandler(new AzureReleaseRepository(httpClient), eventBus, audioStream));

            services.AddHostedService<RaspberryBridge>();
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };

            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri("https://atdevops.azure.intern/NgCollection/Devlab/_apis/");
            var encodedAuth = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("AZURE_TOKEN") ?? "lel");
            Console.WriteLine($"Token: {Environment.GetEnvironmentVariable("AZURE_TOKEN") ?? "lel"}");
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedAuth));
            return httpClient;
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