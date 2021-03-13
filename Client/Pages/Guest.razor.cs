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
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", Branding.WindowTitle);
        }

        [Parameter]
        public string Session
        {
            get;
            set;
        }

        private const string EditGuestNameText = "edit your name";
        private const string SaveGuestNameText = "save your name";

        public string EditGuestNameLinkText
        {
            get;
            private set;
        }

        public bool IsEditingGuestName
        {
            get;
            private set;
        }

        public string GuestName
        {
            get;
            private set;
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
                Handler.GuestInfo.Message.CustomName = GuestName;
                GuestName = Handler.GuestInfo.Message.DisplayName;
                await Handler.GuestInfo.Save();
                await Handler.AnnounceName();
            }
        }

        public SignalRGuest Handler
        {
            get;
            private set;
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        public bool ShowNoSessionMessage
        {
            get;
            private set;
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");

            if (string.IsNullOrEmpty(Session))
            {
                ShowNoSessionMessage = true;
            }
            else
            {
                var success = Guid.TryParse(Session, out Guid guid);

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
                        Session);

                    Handler.UpdateUi += HandlerUpdateUi;
                    await Handler.Connect();

                    GuestName = Handler.GuestInfo.Message.DisplayName;
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
    }
}