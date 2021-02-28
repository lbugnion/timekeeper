using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.HelloWorld;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace Timekeeper.Client.Pages.HelloWorld
{
    public partial class Host : IDisposable
    {
        public const string SendMessageInputId = "send-message-input";

        [Parameter]
        public string ResetSession
        {
            get;
            set;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", "Hello World Backstage Channel");
            await JSRuntime.InvokeVoidAsync("branding.setBranding", "css/hello-world.css");
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

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask 
        { 
            get; 
            set; 
        }

        protected override async Task OnInitializedAsync()
        {
            Today = new Days(Log);

            //var authState = await AuthenticationStateTask;

            //if (!authState.User.Identity.IsAuthenticated)
            //{
            //    Log.LogWarning("Unauthenticated");
            //    return;
            //}

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
            await Handler.Connect("HelloWorldClocksTemplate", ResetSession == "reset");
            SessionName = Handler.CurrentSession.SessionName;
        }

        public async void Dispose()
        {
            Log.LogTrace("Dispose");

            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
                await Handler.Disconnect();
            }
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

        public void ConfigureClock(Clock clock)
        {
            ConfigureClock(clock.Message.ClockId);
        }

        public void ConfigureClock(string clockId)
        {
            if (Handler.PrepareClockToConfigure(clockId))
            {
                Nav.NavigateTo("/helloworld-backstage/configure");
            }
        }

        public async void HandleKeyPress(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await Handler.SendMessage();
                await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
            }
        }
    }
}