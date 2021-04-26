using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model;

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

        public Days Today
        {
            get;
            set;
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

            Today = new Days(Log);

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
            await Session.GetSessions(Log);
            Log.LogInformation("OnInitializedAsync ->");
        }
    }
}