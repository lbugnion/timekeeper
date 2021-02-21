﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using Timekeeper.Client.Model;
using Microsoft.AspNetCore.Components;

namespace Timekeeper.Client.Pages
{
    public partial class Configure
    {
        public StartClockMessage CurrentClockMessage
        {
            get;
            set;
        }

        public Session CurrentSession
        {
            get;
            set;
        }

        public EditContext CurrentEditContext
        {
            get;
            private set;
        }

        protected override void OnInitialized()
        {
            Log.LogInformation("-> Configure.OnInitialized");

            CurrentSession = Program.ClockToConfigure.CurrentSession;
            CurrentClockMessage = Program.ClockToConfigure.CurrentClock.Message;
            Program.ClockToConfigure = null;

            CurrentEditContext = new EditContext(CurrentClockMessage);
            CurrentEditContext.OnValidationStateChanged += CurrentEditContextOnValidationStateChanged;

            Log.LogInformation("OnInitialized ->");
        }

        private async void CurrentEditContextOnValidationStateChanged(object sender, ValidationStateChangedEventArgs e)
        {
            Log.LogInformation("-> CurrentEditContextOnValidationStateChanged");

            if (CurrentEditContext.GetValidationMessages().Count() == 0)
            {
                Log.LogTrace("Saving");
                await CurrentSession.Save();
            }
        }
    }
}