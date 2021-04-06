using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class HostView
    {
        private const string EditSessionNameText = "edit session name";
        private const string SaveSessionNameText = "save session name";
        private const string ShowGuestsText = "show";
        private const string HideGuestsText = "hide";
        public const string SendMessageInputId = "send-message-input";

        public int AnonymousGuests
        {
            get
            {
                return Handler.ConnectedGuests.Count(g => string.IsNullOrEmpty(g.CustomName));
            }
        }

        public string EditSessionNameLinkText
        {
            get;
            private set;
        }

        public string GuestListLinkText
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

        public bool IsEditingSessionName
        {
            get;
            private set;
        }

        public bool IsGuestListExpanded
        {
            get;
            private set;
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

        [Parameter]
        public string SessionName
        {
            get;
            set;
        }

        public MobileHandler Mobile
        {
            get;
            private set;
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> HostView.OnInitializedAsync");
            IsEditingSessionName = false;
            SessionName = "Loading...";
            EditSessionNameLinkText = EditSessionNameText;
            GuestListLinkText = ShowGuestsText;

            Mobile = await new MobileHandler().Initialize(JSRuntime);
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

        public void NavigateToSession()
        {
            Handler.ResetState();
            Nav.NavigateTo("/session");
        }

        public void LogOut()
        {
            Nav.NavigateTo("/.auth/logout?post_logout_redirect_uri=/", forceLoad: true);
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

                if (string.IsNullOrEmpty(SessionName))
                {
                    Handler.CurrentSession.ResetName();
                    SessionName = Handler.CurrentSession.SessionName;
                }
                else
                {
                    Handler.CurrentSession.SessionName = SessionName;
                }

                await Handler.SaveSession();
            }
        }

        public async void HandleFocus()
        {
            await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
        }

        public async void HandleKeyPress(KeyboardEventArgs args)
        {
            if (args.CtrlKey)
            {
                await Handler.SendInputMessage();
                await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
            }
        }

        public void ToggleIsGuestListExpanded()
        {
            IsGuestListExpanded = !IsGuestListExpanded;
            GuestListLinkText = IsGuestListExpanded ? HideGuestsText : ShowGuestsText;
        }
    }
}