﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.HelloWorld;

namespace Timekeeper.Client.Pages.HelloWorld
{
    public partial class Guest : IDisposable
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
            Today = new Days(Log);

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

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");

            Today = new Days(Log);

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