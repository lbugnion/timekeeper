using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Timekeeper.DataModel;

// Set version number for the assembly.
[assembly: AssemblyVersion("0.1.8888.*")]

namespace TimekeeperClient
{
    public class Program
    {
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

        public static GroupInfo GroupInfo
        {
            get;
            set;
        }

        public static async Task Main(string[] args)
        {
            GroupInfo = new GroupInfo
            {
                UserId = Guid.NewGuid().ToString()
            };

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Logging.AddConfiguration(
                builder.Configuration.GetSection("Logging"));

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Logging
                .ClearProviders()
                .AddProvider(new TimekeeperLoggerProvider(new TimekeeperLoggerConfiguration
                {
                    MinimumLogLevel = LogLevel.Trace
                }));

            await builder.Build().RunAsync();
        }
    }
}