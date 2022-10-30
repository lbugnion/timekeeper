using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.Polls;

namespace Timekeeper.Client.Pages
{
    public partial class ManagePolls : IDisposable
    {
        public const string VisibilityInvisible = "invisible";
        public const string VisibilityVisible = "visible";
        public static readonly string ManagePollsSessionId = nameof(ManagePolls) + "SessionId";

        public PollHost Handler
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

        public string WindowTitle
        {
            get
            {
                if (Handler == null
                    || Handler.CurrentSession == null
                    || string.IsNullOrEmpty(Handler.CurrentSession.SessionName))
                {
                    return Branding.PollsPageTitle;
                }

                return $"{Handler.CurrentSession.SessionName} {Branding.PollsPageTitle}";
            }
        }

        private async void HandlerUpdateUi(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogTrace("-> ManagePolls.OnInitialized");
            Log.LogDebug($"SessionId: {SessionId}");

            UiVisibility = VisibilityVisible;

            Handler = new PollHost(
                Config,
                LocalStorage,
                Log,
                Http,
                Nav,
                Session,
                SessionId);

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