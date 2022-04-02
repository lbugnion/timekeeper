using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Reflection;
using System.Threading.Tasks;
using Timekeeper.Client.Model;

namespace Timekeeper.Client.Pages
{
    public partial class About
    {
        public string Beta
        {
            get;
            private set;
        }

        public string ClientVersion
        {
            get;
            private set;
        }

        public string Environment
        {
            get;
            private set;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("branding.setTitle", $"{Branding.WindowTitle} : About");
            }
        }

        protected override void OnInitialized()
        {
            Log.LogInformation("-> About.OnInitialized");
            (ClientVersion, Beta, Environment) = MakeClientVersion(Config, Log);
        }

        public static (string clientVersion, string alphaBeta, string environment) MakeClientVersion(
            IConfiguration config,
            ILogger log)
        {
            string clientVersion, alphaBeta, environment;

            try
            {
                var version = Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version;

                log.LogDebug($"Full version: {version}");
                clientVersion = $"V{version.ToString(4)}";
                alphaBeta = string.Empty;
                log.LogDebug($"clientVersion: {clientVersion}");

                if (version.Revision == 8888)
                {
                    clientVersion = $"V{version.ToString(3)}";
                    alphaBeta = "Alpha";
                }

                if (version.Revision == 9999)
                {
                    clientVersion = $"V{version.ToString(3)}";
                    alphaBeta = "Beta";
                }

                environment = $"| {config.GetValue<string>("Environment")}";
                if (environment == "| Production")
                {
                    environment = string.Empty;
                }

                log.LogDebug($"environment: {environment}");

                return (clientVersion, alphaBeta, environment);
            }
            catch
            {
                log.LogWarning($"Assembly not found");
                return ("N/A", string.Empty, string.Empty);
            }
        }
    }
}