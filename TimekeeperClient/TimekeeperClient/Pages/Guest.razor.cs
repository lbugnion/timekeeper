﻿using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using TimekeeperClient.Model;

namespace TimekeeperClient.Pages
{
    public partial class Guest : IDisposable
    {
        [Parameter]
        public string Session
        {
            get;
            set;
        }

        public string BackgroundClassName
        {
            get;
            private set;
        }

        public SignalRGuest Handler
        {
            get;
            private set;
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            if (Handler.IsRed)
            {
                BackgroundClassName = Index.RedBackgroundClassName;
            }
            else if (Handler.IsYellow)
            {
                BackgroundClassName = Index.YellowBackgroundClassName;
            }
            else if (Handler.IsClockRunning)
            {
                BackgroundClassName = Index.RunningBackgroundClassName;
            }
            else
            {
                BackgroundClassName = Index.NormalBackgroundClassName;
            }

            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            BackgroundClassName = Index.NormalBackgroundClassName;

            Handler = new SignalRGuest(
                Config,
                LocalStorage,
                Log,
                Http,
                Session);

            Handler.UpdateUi += HandlerUpdateUi;
            await Handler.Connect();
        }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }
        }
    }
}