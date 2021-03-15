using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
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
        private const string EditSessionNameText = "edit session name";
        private const string SaveSessionNameText = "save session name";
        public const string SendMessageInputId = "send-message-input";
        private const string KeepDeviceAwakeText = "Keep device awake";
        private const string AllowDeviceToSleepText = "Allow device to sleep";

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

        public bool IsMobile
        {
            get;
            private set;
        }

        protected override async Task OnInitializedAsync()
        {
            IsEditingSessionName = false;
            SessionName = "Loading...";
            EditSessionNameLinkText = EditSessionNameText;
            GuestListLinkText = "show";

            IsMobile = await JSRuntime.InvokeAsync<bool>("nosleep.isMobile");

            NoSleepButtonText = KeepDeviceAwakeText;
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

        public async void HandleFocus()
        {
            await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
        }

        public async void HandleKeyPress(KeyboardEventArgs args)
        {
            if (args.CtrlKey)
            {
                await Handler.SendMessage();
                await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
            }
        }

        public void ToggleIsGuestListExpanded()
        {
            IsGuestListExpanded = !IsGuestListExpanded;
            GuestListLinkText = IsGuestListExpanded ? "hide" : "show";
        }

        private bool _isNoSleepActive;

        public string NoSleepButtonText
        {
            get;
            private set;
        }

        public void ToggleNoSleep(object sender)
        {
            if (_isNoSleepActive)
            {
                JSRuntime.InvokeVoidAsync("nosleep.enableDisableNoSleep", false);
                _isNoSleepActive = false;
                NoSleepButtonText = KeepDeviceAwakeText;
            }
            else
            {
                JSRuntime.InvokeVoidAsync("nosleep.enableDisableNoSleep", true);
                _isNoSleepActive = true;
                NoSleepButtonText = AllowDeviceToSleepText;
            }
        }
    }
}