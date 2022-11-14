using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

#if !DEBUG
using Timekeeper.Client.Model;
#endif

namespace Timekeeper.Client.Pages
{
    public partial class SessionSelection
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask
        {
            get;
            set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        private async Task CheckSetNewSession()
        {
            var newSessionId = await Session.CheckSetNewSession(Log);

            if (!string.IsNullOrEmpty(newSessionId))
            {
                Nav.NavigateTo($"/host/{newSessionId}");
            }
        }

        private async Task Duplicate(string sessionId)
        {
            try
            {
                var newSessionId = await Session.Duplicate(sessionId, Log);

                if (!string.IsNullOrEmpty(newSessionId))
                {
                    Nav.NavigateTo($"/host/{newSessionId}");
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
                Nav.NavigateTo($"/host/{sessionId}");
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

#if !DEBUG
            if (Branding.MustAuthorize)
            {
                var authState = await AuthenticationStateTask;

                if (authState == null
                    || authState.User == null
                    || authState.User.Identity == null
                    || !authState.User.Identity.IsAuthenticated)
                {
                    Log.LogWarning("Unauthenticated");
                    Nav.NavigateTo("/host");
                    return;
                }
            }
#endif

            Session.InitializeContext(Log);

            try
            {
                await Session.GetSessions(Log);
            }
            catch (Exception ex)
            {
                Log.LogError($"Cannot get sessions: {ex.Message}");
                ErrorMessage = "Error getting sessions";
            }

            Log.LogInformation("OnInitializedAsync ->");
        }
    }
}