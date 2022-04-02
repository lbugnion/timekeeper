using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model;

namespace Timekeeper.Client.Pages
{
    public partial class Guest : IDisposable
    {
        private const string EditGuestNameText = "edit your name";

        private const string SaveGuestNameText = "save your name";

        private const string VisibilityInvisible = "invisible";
        private const string VisibilityVisible = "visible";

        public string EditGuestNameLinkText
        {
            get;
            private set;
        }

        public string GuestName
        {
            get;
            private set;
        }

        public SignalRGuest Handler
        {
            get;
            private set;
        }

        public bool IsEditingGuestName
        {
            get;
            private set;
        }

        public MobileHandler Mobile
        {
            get;
            private set;
        }

        [Parameter]
        public string SessionId
        {
            get;
            set;
        }

        public bool ShowNoSessionMessage
        {
            get;
            private set;
        }

        public string ToggleButtonClass
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
                    || string.IsNullOrEmpty(Handler.CurrentSession.SessionName)
                    || Handler.CurrentSession.SessionName == Branding.GuestPageTitle)
                {
                    return Branding.GuestPageTitle;
                }

                return $"{Handler.CurrentSession.SessionName} {Branding.GuestPageTitle}";
            }
        }

        private async void HandlerUpdateUi(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);
            StateHasChanged();
        }

        private void ToggleFocus()
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");

            UiVisibility = VisibilityVisible;

            if (string.IsNullOrEmpty(SessionId))
            {
                ShowNoSessionMessage = true;
            }
            else
            {
                var success = Guid.TryParse(SessionId, out Guid guid);

                if (!success
                    || guid == Guid.Empty)
                {
                    ShowNoSessionMessage = true;
                }
                else
                {
                    IsEditingGuestName = false;
                    GuestName = "Loading...";
                    EditGuestNameLinkText = EditGuestNameText;

                    Handler = new SignalRGuest(
                        Config,
                        LocalStorage,
                        Log,
                        Http,
                        SessionId,
                        Session);

                    Handler.UpdateUi += HandlerUpdateUi;
                    await Handler.Connect();

                    await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);

                    GuestName = Handler.PeerInfo.Message.DisplayName;
                    Mobile = await new MobileHandler().Initialize(JSRuntime);

                    Log.LogDebug($"GuestName: {GuestName}");
                }
            }

            Log.LogInformation("OnInitializedAsync ->");
        }

        public async void Dispose()
        {
            Log.LogTrace("Dispose");

            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }

            await Task.Run(async () =>
            {
                await Handler.Disconnect();
            });
        }

        public async Task EditGuestName()
        {
            IsEditingGuestName = !IsEditingGuestName;

            if (IsEditingGuestName)
            {
                EditGuestNameLinkText = SaveGuestNameText;
            }
            else
            {
                EditGuestNameLinkText = EditGuestNameText;
                await Handler.SetCustomUserName(GuestName);
                GuestName = Handler.PeerInfo.Message.DisplayName;
            }
        }
    }
}