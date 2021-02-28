using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Reflection;
using System.Threading.Tasks;
using Timekeeper.Client.Model.HelloWorld;

namespace Timekeeper.Client.Pages.HelloWorld
{
    public partial class Index
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", "Hello World Backstage Channel");
            await JSRuntime.InvokeVoidAsync("branding.setBranding", "css/hello-world.css");
        }

        public Days Today
        {
            get;
            set;
        }

        [Parameter]
        public string Session
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
            Log.LogInformation("-> Index.OnInitialized");
            Log.LogDebug($"Session: {Session}");

            if (!string.IsNullOrEmpty(Session))
            {
                Nav.NavigateTo($"/helloworld-backstage/guest/{Session}");
                return;
            }

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

        public void LogInHost()
        {
            Nav.NavigateTo("/helloworld-backstage/host");
        }
    }
}