using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.Polls;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class DisplayPolls
    {
        private string _role;

        public PollGuest Handler
        {
            get;
            private set;
        }

        public MobileHandler Mobile
        {
            get;
            private set;
        }

        [Parameter]
        public string Role
        {
            get => _role;
            set
            {
                _role = value;

                if (Handler != null)
                {
                    Handler.Role = value;
                }
            }
        }

        [Parameter]
        public string SessionId
        {
            get;
            set;
        }

        public bool ShowNoSessionMessage
        {
            get;
            private set;
        }

        public string WindowTitle
        {
            get
            {
                if (Handler == null
                    || Handler.CurrentSession == null
                    || string.IsNullOrEmpty(Handler.CurrentSession.SessionName)
                    || Handler.CurrentSession.SessionName == Branding.PollsPageTitle)
                {
                    return Branding.PollsPageTitle;
                }

                return $"{Handler.CurrentSession.SessionName} {Branding.PollsPageTitle}";
            }
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");

            if (string.IsNullOrEmpty(SessionId))
            {
                ShowNoSessionMessage = true;
            }
            else
            {
                var success = Guid.TryParse(SessionId, out Guid guid);

                if (!success
                    || guid == Guid.Empty)
                {
                    ShowNoSessionMessage = true;
                }
                else
                {
                    Handler = new PollGuest(
                        Config,
                        LocalStorage,
                        Log,
                        Http,
                        SessionId,
                        Session)
                    {
                        Role = _role
                    };

                    Handler.UpdateUi += HandlerUpdateUi;
                    await Handler.Connect();

                    await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);

                    Mobile = await new MobileHandler().Initialize(JSRuntime);
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

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }

        public string GetPollClass(Poll poll)
        {
            if (poll.IsVotingOpen)
            {
                return "poll-published";
            }

            return "poll-closed";
        }
    }
}