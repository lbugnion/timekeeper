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

        private void CheckSetNewSession()
        {
            if (Session.CheckSetNewSession())
            {
                Nav.NavigateTo("/host");
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
    }
}
