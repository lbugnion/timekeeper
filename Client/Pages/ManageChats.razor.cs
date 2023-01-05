using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model.Chats;

namespace Timekeeper.Client.Pages
{
    public partial class ManageChats : IDisposable
    {
        public const string VisibilityInvisible = "invisible";
        public const string VisibilityVisible = "visible";

        public ChatHost Handler
        {
            get;
            set;
        }

        [Parameter]
        public string SessionId
        {
            get;
            set;
        }

        public string UiVisibility
        {
            get;
            set;
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogTrace("-> ManageChats.OnInitializedAsync");
            Log.LogDebug($"SessionId: {SessionId}");

            UiVisibility = VisibilityVisible;

            Handler = new ChatHost(
                Config,
                LocalStorage,
                Log,
                Http,
                Nav,
                Session,
                SessionId);

            Handler.ChatProxy.SetNewChat();

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

            Handler.UpdateUi += HandlerUpdateUi;
            Handler.RequestRefresh += HandlerRequestRefresh;
            await Handler.Connect();
        }

        private async void HandlerRequestRefresh(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("host.refreshPage");
        }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
                Handler.RequestRefresh -= HandlerRequestRefresh;
            }
        }

        public void ToggleFocus()
        {
            Log.LogTrace("-> ToggleFocus");

            if (UiVisibility == VisibilityVisible)
            {
                Log.LogTrace("Setting Invisible");
                UiVisibility = VisibilityInvisible;
            }
            else
            {
                Log.LogTrace("Setting Visible");
                UiVisibility = VisibilityVisible;
            }
        }
    }
}