using AzureStaticWebApps.Blazor.Authentication;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Timekeeper.Client.Model;

// Set version number for the assembly.
[assembly: AssemblyVersion("0.6.0.0")]

namespace Timekeeper.Client
{
    public class Program
    {
        // TODO Use dependency injection?
        public static ConfigureClock ClockToConfigure
        {
            get;
            internal set;
        }

        public static bool IsExperimental
        {
            get
            {
                var version = Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version;

                return version.Build == 8888;
            }
        }

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Logging.AddConfiguration(
                builder.Configuration.GetSection("Logging"));

            builder.Logging
                .ClearProviders()
                .AddProvider(new TimekeeperLoggerProvider(new TimekeeperLoggerConfiguration
                {
                    MinimumLogLevel = LogLevel.Trace
                }));

            var provider = builder.Services.BuildServiceProvider();

            builder.Services
                .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                .AddStaticWebAppsAuthentication();

            builder.Services.AddBlazoredLocalStorage();

            builder.Services
                .AddScoped<SessionHandler>();

            await builder.Build().RunAsync();
        }
    }
}