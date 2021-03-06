﻿using Microsoft.AspNetCore.Components;
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

        private const string VisibilityVisible = "visible";
        private const string VisibilityInvisible = "invisible";

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

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", Branding.WindowTitle);
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
                Handler.PeerInfo.Message.CustomName = GuestName;
                GuestName = Handler.PeerInfo.Message.DisplayName;
                await Handler.SavePeerInfo();
                await Handler.AnnounceName();
            }
        }

        public string UiVisibility
        {
            get;
            set;
        }

        public string ToggleButtonClass
        {
            get;
            set;
        }

        private void ToggleFocus()
        {
            Log.LogTrace("HIGHLIGHT---> ToggleFocus");

            if (UiVisibility == VisibilityVisible)
            {
                Log.LogTrace("HIGHLIGHT--Setting Invisible");
                UiVisibility = VisibilityInvisible;
            }
            else
            {
                Log.LogTrace("HIGHLIGHT--Setting Visible");
                UiVisibility = VisibilityVisible;
            }
        }
    }
}