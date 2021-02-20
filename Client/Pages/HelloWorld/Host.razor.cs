using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.HelloWorld;

namespace Timekeeper.Client.Pages.HelloWorld
{
    public partial class Host : IDisposable
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("setTitle", "Hello World Backstage Channel");
            await JSRuntime.InvokeVoidAsync("setBranding", "css/hello-world.css");
        }

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
                return $"{Nav.BaseUri}helloworld-backstage/guest/{Handler.CurrentSession.SessionId}";
            }
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Today = new Days(Log);

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

        public void ConfigureSession()
        {
            Nav.NavigateTo("/helloworld-backstage/configure");
        }

        public void CreateNewSession()
        {
            Nav.NavigateTo("/helloworld-backstage/host", forceLoad: true);
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
    }
}