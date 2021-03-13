using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class HostView
    {
        public const string SendMessageInputId = "send-message-input";

        public Days Today
        {
            get;
            set;
        }

        public bool IsEditingSessionName
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

        [Parameter]
        public SignalRHost Handler
        {
            get;
            set;
        }

        public async void HandleKeyPress(KeyboardEventArgs args)
        {
            if (args.CtrlKey)
            {
                await Handler.SendMessage();
                await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
            }
        }

        public async void HandleFocus()
        {
            await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
        }

        public void ConfigureClock(Clock clock)
        {
            ConfigureClock(clock.Message.ClockId);
        }

        public void ConfigureClock(string clockId)
        {
            if (Handler.PrepareClockToConfigure(clockId))
            {
                Nav.NavigateTo("/configure");
            }
        }

        public void CreateNewSession()
        {
            Nav.NavigateTo("/host", forceLoad: true);
        }

        protected override void OnInitialized()
        {
            Today = new Days(Log);
            IsEditingSessionName = false;
            SessionName = "Loading...";
            EditSessionNameLinkText = EditSessionNameText;
            GuestListLinkText = "show";
        }

        public int AnonymousGuests
        {
            get
            {
                return Handler.ConnectedGuests.Count(g => string.IsNullOrEmpty(g.CustomName));
            }
        }

        public IList<GuestMessage> NamedGuests
        {
            get
            {
                return Handler.ConnectedGuests
                    .Where(g => !string.IsNullOrEmpty(g.CustomName))
                    .ToList();
            }
        }

        public bool IsGuestListExpanded
        {
            get;
            private set;
        }

        public string GuestListLinkText
        {
            get;
            private set;
        }

        public void ToggleIsGuestListExpanded()
        {
            IsGuestListExpanded = !IsGuestListExpanded;
            GuestListLinkText = IsGuestListExpanded ? "hide" : "show";
        }

        public async Task EditSessionName()
        {
            IsEditingSessionName = !IsEditingSessionName;

            if (IsEditingSessionName)
            {
                EditSessionNameLinkText = SaveSessionNameText;
            }
            else
            {
                EditSessionNameLinkText = EditSessionNameText;
                Handler.CurrentSession.SessionName = SessionName;
                await Handler.CurrentSession.Save(Log);
            }
        }

        private const string EditSessionNameText = "edit session name";
        private const string SaveSessionNameText = "save session name";

        public string EditSessionNameLinkText
        {
            get;
            private set;
        }

        [Parameter]
        public string SessionName
        {
            get;
            set;
        }
    }
}
