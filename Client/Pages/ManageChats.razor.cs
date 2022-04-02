﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model.Chats;

namespace Timekeeper.Client.Pages
{
    public partial class ManageChats : IDisposable
    {
        public const string VisibilityVisible = "visible";
        public const string VisibilityInvisible = "invisible";

        public string UiVisibility
        {
            get;
            set;
        }

        public ChatHost Handler
        {
            get;
            set;
        }

        public void ToggleFocus()
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

        [Parameter]
        public string SessionId
        {
            get;
            set;
        }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogTrace("-> ManageChats.OnInitializedAsync");
            Log.LogDebug($"SessionId: {SessionId}");

            UiVisibility = VisibilityVisible;

            Handler = new ChatHost(
                Config,
                LocalStorage,
                Log,
                Http,
                Nav,
                Session,
                SessionId);

            Handler.ChatProxy.SetNewChat();

            Log.LogTrace("Check authorization");
            await Handler.CheckAuthorize();

            if (Handler.IsAuthorized != null
                && !Handler.IsAuthorized.Value)
            {
                Log.LogError("No authorization");
                return;
            }
            else if (!Handler.IsConnected
                && Handler.IsInError)
            {
                Log.LogError("Offline");
                return;
            }

            Handler.UpdateUi += HandlerUpdateUi;
            await Handler.Connect();
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }
    }
}