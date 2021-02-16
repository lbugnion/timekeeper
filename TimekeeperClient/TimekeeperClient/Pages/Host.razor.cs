using System;
using System.Threading.Tasks;
using TimekeeperClient.Model;

namespace TimekeeperClient.Pages
{
    public partial class Host : IDisposable
    {
        public bool IsEditingSessionName
        {
            get;
            private set;
        }

        public string BackgroundClassName
        {
            get;
            private set;
        }

        public SignalRHost Handler
        {
            get;
            private set;
        }

        public string SessionName
        {
            get;
            private set;
        }

        public string GuestUrl
        {
            get
            {
                return $"{Nav.BaseUri}{Handler.CurrentSession.SessionId}";
            }
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
            IsEditingSessionName = false;
            SessionName = "Loading...";
            EditSessionNameLinkText = EditSessionNameText;

            BackgroundClassName = Index.NormalBackgroundClassName;

            Handler = new SignalRHost(
                Config,
                LocalStorage,
                Log,
                Http);

            Handler.UpdateUi += HandlerUpdateUi;
            await Handler.Connect();
            SessionName = Handler.CurrentSession.SessionName;
        }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }
        }

        public async Task EditSessionName()
        {
            IsEditingSessionName = !IsEditingSessionName;

            if (IsEditingSessionName)
            {
                EditSessionNameLinkText = SaveSessionNameText;
            }
            else
            {
                EditSessionNameLinkText = EditSessionNameText;
                Handler.CurrentSession.SessionName = SessionName;
                await Handler.SaveCurrentSession();
            }
        }

        private const string EditSessionNameText = "edit session name";
        private const string SaveSessionNameText = "save session name";

        public string EditSessionNameLinkText
        {
            get;
            private set;
        }
    }
}