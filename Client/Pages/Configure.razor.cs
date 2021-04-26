using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class Configure : IDisposable
    {
        public StartClockMessage CurrentClockMessage
        {
            get;
            set;
        }

        public EditContext CurrentEditContext
        {
            get;
            private set;
        }

        public SignalRHost Host
        {
            get;
            set;
        }

        public Days Today
        {
            get;
            set;
        }

        private async void CurrentEditContextOnValidationStateChanged(object sender, ValidationStateChangedEventArgs e)
        {
            Log.LogInformation("-> CurrentEditContextOnValidationStateChanged");

            if (CurrentEditContext.GetValidationMessages().Count() == 0)
            {
                Log.LogTrace("Saving");

                if (!await Host.SaveSession())
                {
                    return;
                }

                await Host.UpdateRemoteHosts(
                    UpdateAction.UpdateClock,
                    null,
                    CurrentClockMessage,
                    null);
            }
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", Branding.WindowTitle);
        }

        protected override void OnInitialized()
        {
            Log.LogInformation("-> Configure.OnInitialized");

            if (Program.ClockToConfigure == null)
            {
                Nav.NavigateTo("/");
                return;
            }

            Today = new Days(Log);
            Host = Program.ClockToConfigure.Host;
            Host.UpdateUi += HandlerUpdateUi;

            CurrentClockMessage = Program.ClockToConfigure.CurrentClock.Message;
            CurrentEditContext = new EditContext(CurrentClockMessage);
            CurrentEditContext.OnValidationStateChanged += CurrentEditContextOnValidationStateChanged;

            Log.LogInformation("OnInitialized ->");
        }

        public void Dispose()
        {
            Host.UpdateUi -= HandlerUpdateUi;
        }
    }
}