using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using Timekeeper.Client.Model;

namespace Timekeeper.Client.Pages
{
    public partial class Host : IDisposable
    {
        public bool IsEditingSessionName
        {
            get;
            private set;
        }

        public SignalRHost Handler
        {
            get;
            private set;
        }

        public string SessionName
        {
            get;
            private set;
        }

        public string GuestUrl
        {
            get
            {
                return $"{Nav.BaseUri}{Handler.CurrentSession.SessionId}";
            }
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            IsEditingSessionName = false;
            SessionName = "Loading...";
            EditSessionNameLinkText = EditSessionNameText;
            GuestListLinkText = "show";

            Handler = new SignalRHost(
                Config,
                LocalStorage,
                Log,
                Http);

            Handler.UpdateUi += HandlerUpdateUi;
            await Handler.Connect();
            SessionName = Handler.CurrentSession.SessionName;
        }

        public async void Dispose()
        {
            Log.LogTrace("Dispose");

            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }

            await Handler.Disconnect();
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
                await Handler.CurrentSession.Save();
            }
        }

        private const string EditSessionNameText = "edit session name";
        private const string SaveSessionNameText = "save session name";

        public string EditSessionNameLinkText
        {
            get;
            private set;
        }

        public void CreateNewSession()
        {
            Nav.NavigateTo("/host", forceLoad: true);
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
    }
}