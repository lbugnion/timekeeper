using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Timekeeper.Client.Pages
{
    public partial class SessionSelection
    {
        public string ErrorMessage
        {
            get;
            private set;
        }

        private async Task CheckSetNewSession()
        {
            if (await Session.CheckSetNewSession(Log))
            {
                Nav.NavigateTo("/host");
            }
        }

        private async Task Duplicate(string sessionId)
        {
            try
            {
                if (await Session.Duplicate(sessionId, Log))
                {
                    Nav.NavigateTo("/host");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task DeleteSession(string sessionId)
        {
            try
            {
                await Session.DeleteSession(sessionId, Log);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task SelectSession(string sessionId)
        {
            try
            {
                await Session.SelectSession(sessionId, Log);
                Nav.NavigateTo("/host");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");
            Log.LogDebug($"State: {Session.State}");

            if (Session.State != 1)
            {
                Nav.NavigateTo("/host");
                return;
            }

            Session.InitializeContext(Log);
            await Session.GetSessions(Log);
            Log.LogInformation("OnInitializedAsync ->");
        }
    }
}