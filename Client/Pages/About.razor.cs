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

        public string Beta
        {
            get;
            private set;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", $"{Branding.WindowTitle} : About");
        }

        protected override void OnInitialized()
        {
            Log.LogInformation("-> About.OnInitialized");

            try
            {
                var version = Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version;

                Log.LogDebug($"Full version: {version}");
                ClientVersion = $"V{version.ToString(4)}";
                Log.LogDebug($"clientVersion: {ClientVersion}");

                if (version.Build == 8888)
                {
                    ClientVersion = $"V{version.ToString(2)}";
                    Beta = "Alpha";
                }

                if (version.Build == 9999)
                {
                    ClientVersion = $"V{version.ToString(2)}";
                    Beta = "Beta";
                }

                var environment = Config.GetValue<string>("Environment");
                if (environment == "Production")
                {
                    environment = string.Empty;
                }

                Environment = environment;
            }
            catch
            {
                Log.LogWarning($"Assembly not found");
                ClientVersion = "N/A";
            }
        }
    }
}