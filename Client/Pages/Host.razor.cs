using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model;

namespace Timekeeper.Client.Pages
{
    public partial class Host : IDisposable
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask
        {
            get;
            set;
        }

        public SignalRHost Handler
        {
            get;
            private set;
        }

        public string SessionName
        {
            get
            {
                if (Handler == null
                    || Handler.CurrentSession == null)
                {
                    return "No session";
                }

                return Handler.CurrentSession.SessionName;
            }
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", Branding.WindowTitle);
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> Host.OnInitializedAsync");

#if !DEBUG
            if (Branding.MustAuthorize)
            {
                var authState = await AuthenticationStateTask;

                if (authState == null
                    || authState.User == null
                    || authState.User.Identity == null
                    || !authState.User.Identity.IsAuthenticated)
                {
                    Log.LogWarning("Unauthenticated");
                    return;
                }
            }
#endif

            if (Program.ClockToConfigure == null)
            {
                Handler = new SignalRHost(
                    Config,
                    LocalStorage,
                    Log,
                    Http,
                    Nav,
                    Session);
            }
            else
            {
                Handler = Program.ClockToConfigure.Host;
                Program.ClockToConfigure.Host = null;
            }

            Log.LogTrace("Check authorization");
            await Handler.CheckAuthorize();

            if (Handler.IsAuthorized != null
                && !Handler.IsAuthorized.Value)
            {
                Log.LogError("No authorization");
                return;
            }
            else if (Handler.IsOffline != null
                && Handler.IsOffline.Value)
            {
                Log.LogError("Offline");
                return;
            }

            if (Branding.AllowSessionSelection)
            {
                await Handler.CheckState();
            }

            Log.LogDebug($"Current session is null: {Handler.CurrentSession == null}");

            if (Handler.CurrentSession != null)
            {
                Log.LogDebug($"Handler.CurrentSession.Clocks.Count: {Handler.CurrentSession.Clocks.Count}");

                foreach (var clock in Handler.CurrentSession.Clocks)
                {
                    Log.LogDebug($"Clock {clock.Message.Label}");
                }
            }

            Handler.UpdateUi += HandlerUpdateUi;
            await Handler.Connect();
        }

        public async void Dispose()
        {
            Log.LogTrace("Dispose");

            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;

                if (Program.ClockToConfigure == null)
                {
                    Log.LogTrace("Disconnecting");
                    await Handler.Disconnect();
                }
            }
        }
    }
}