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

        [Parameter]
        public string SessionId { get; set; }

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

        public string WindowTitle
        {
            get
            {
                if (Handler == null
                    || Handler.CurrentSession == null
                    || string.IsNullOrEmpty(Handler.CurrentSession.SessionName))
                {
                    return Branding.MainPageTitle;
                }

                return $"{Handler.CurrentSession.SessionName} {Branding.MainPageTitle}";
            }
        }

        private async void HandlerUpdateUi(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("branding.setTitle", Branding.WindowTitle);
            }
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
                    Session,
                    SessionId);
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
            else if (!Handler.IsConnected
                && Handler.IsInError)
            {
                Log.LogError("Offline");
                return;
            }

            if (Branding.AllowSessionSelection)
            {
                await Handler.CheckState();
            }

            Handler.UpdateUi += HandlerUpdateUi;
            Handler.RequestRefresh += HandlerRequestRefresh;
            await Handler.Connect();
        }

        private async void HandlerRequestRefresh(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("host.refreshPage");
        }

        public async void Dispose()
        {
            Log.LogTrace("Dispose");

            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
                Handler.RequestRefresh -= HandlerRequestRefresh;

                await Handler.DeleteSessionFromStorage();
                await Handler.ResetState();

                if (Program.ClockToConfigure == null)
                {
                    Log.LogTrace("Disconnecting");
                    await Handler.Disconnect();
                }
            }
        }
    }
}