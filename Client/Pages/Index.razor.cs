using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Reflection;
using System.Threading.Tasks;
using Timekeeper.Client.Model;

namespace Timekeeper.Client.Pages
{
    public partial class Index
    {
        public string ClientVersion
        {
            get;
            private set;
        }

        public string Beta
        {
            get;
            private set;
        }

        public string Environment
        {
            get;
            private set;
        }

        [Parameter]
        public string Session
        {
            get;
            set;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("branding.setTitle", Branding.WindowTitle);
            }
        }

        protected override void OnInitialized()
        {
            Log.LogInformation("-> Index.OnInitialized");
            Log.LogDebug($"Session: {Session}");

            if (!string.IsNullOrEmpty(Session))
            {
                Nav.NavigateTo($"/guest/{Session}");
                return;
            }

            (ClientVersion, Beta, Environment) = About.MakeClientVersion(Config, Log);
        }

        public void LogInHost()
        {
            Nav.NavigateTo("/host");
        }
    }
}