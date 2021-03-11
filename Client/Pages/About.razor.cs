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
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", $"{Branding.WindowTitle} : About");
        }

        public Days Today
        {
            get;
            set;
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

        protected override void OnInitialized()
        {
            Log.LogInformation("-> About.OnInitialized");

            try
            {
                Today = new Days(Log);

                var version = Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version;

                Log.LogDebug($"Full version: {version}");
                ClientVersion = $"V{version.ToString(4)}";
                Log.LogDebug($"clientVersion: {ClientVersion}");

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
