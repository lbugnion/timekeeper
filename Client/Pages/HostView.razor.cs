using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Timekeeper.Client.Model;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class HostView
    {
        private const string EditSessionNameText = "edit session name";
        private const string HidePeersText = "hide";
        private const string SaveSessionNameText = "save session name";
        private const string ShowPeersText = "show";
        private string _guestUrlQrCode;
        public const string SendMessageInputId = "send-message-input";

        public string EditSessionNameLinkText
        {
            get;
            private set;
        }

        public string GuestUrl
        {
            get
            {
                return $"{Nav.BaseUri}guest/{Handler.CurrentSession.SessionId}";
            }
        }

        public string GuestUrlQrCode
        {
            get
            {
                if (string.IsNullOrEmpty(_guestUrlQrCode))
                {
                    var url = HttpUtility.UrlEncode(GuestUrl);
                    _guestUrlQrCode = $"{Nav.BaseUri}api/qr?text={url}";

#if DEBUG
                    _guestUrlQrCode = $"http://localhost:7071/api/qr?text={url}";
#endif

                    //Log.LogDebug($"codeUrl: {codeUrl}");
                }

                return _guestUrlQrCode;
            }
        }

        [Parameter]
        public SignalRHost Handler
        {
            get;
            set;
        }

        public bool IsEditingSessionName
        {
            get;
            private set;
        }

        public bool IsPeersListExpanded
        {
            get;
            private set;
        }

        public MobileHandler Mobile
        {
            get;
            private set;
        }

        public IList<PeerMessage> NamedGuests
        {
            get
            {
                return Handler.ConnectedPeers
                    .Where(g => !string.IsNullOrEmpty(g.CustomName))
                    .ToList();
            }
        }

        public string PeerListLinkText
        {
            get;
            private set;
        }

        public string SessionId => Handler.CurrentSession.SessionId;

        [Parameter]
        public string SessionName
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
                    return Branding.MainPageTitle;
                }

                return $"{Handler.CurrentSession.SessionName} {Branding.MainPageTitle}";
            }
        }

        private async Task DoDeleteSession()
        {
            await Handler.DoDeleteSession();
            Nav.NavigateTo("/host", forceLoad: true);
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> HostView.OnInitializedAsync");
            IsEditingSessionName = false;
            EditSessionNameLinkText = EditSessionNameText;
            PeerListLinkText = ShowPeersText;

            await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);

            Mobile = await new MobileHandler().Initialize(JSRuntime);
            Log.LogInformation("HostView.OnInitializedAsync ->");
        }

        public void ConfigureClock(Clock clock)
        {
            ConfigureClock(clock.Message.ClockId);
        }

        public void ConfigureClock(string clockId)
        {
            Log.LogInformation("-> ConfigureClock");

            if (Handler.PrepareClockToConfigure(clockId))
            {
                Nav.NavigateTo("/configure");
            }
        }

        public void CreateNewSession()
        {
            Nav.NavigateTo("/host", forceLoad: true);
        }

        public async Task EditSessionName()
        {
            Log.LogInformation("-> HostView.EditSessionName");

            IsEditingSessionName = !IsEditingSessionName;

            if (IsEditingSessionName)
            {
                EditSessionNameLinkText = SaveSessionNameText;
            }
            else
            {
                EditSessionNameLinkText = EditSessionNameText;

                if (string.IsNullOrEmpty(SessionName))
                {
                    Handler.CurrentSession.ResetName();
                    SessionName = Handler.CurrentSession.SessionName;
                }
                else
                {
                    Handler.CurrentSession.SessionName = SessionName;
                }

                await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);

                await Handler.SaveSession();

                await Handler.UpdateRemoteHosts(
                    UpdateAction.UpdateSessionName,
                    SessionName,
                    null,
                    null);
            }
        }

        public async void HandleFocus()
        {
            await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
        }

        public async void HandleKeyPress(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                if (!args.ShiftKey)
                {
                    await Handler.SendInputMessage();
                    await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
                }
            }
        }

        public void LogOut()
        {
            Nav.NavigateTo("/.auth/logout?post_logout_redirect_uri=/", forceLoad: true);
        }

        public async Task NavigateToSession()
        {
            await Handler.DeleteSessionFromStorage();            
            await Handler.ResetState();
            Nav.NavigateTo("/session");
        }

        public void ToggleIsPeersListExpanded()
        {
            IsPeersListExpanded = !IsPeersListExpanded;
            PeerListLinkText = IsPeersListExpanded ? HidePeersText : ShowPeersText;
        }
    }
}