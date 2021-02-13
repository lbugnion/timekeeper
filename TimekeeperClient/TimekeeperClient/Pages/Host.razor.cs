using System;
using System.Threading.Tasks;
using TimekeeperClient.Model;

namespace TimekeeperClient.Pages
{
    public partial class Host : IDisposable
    {
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

            Handler = new SignalRHost(
                Config,
                LocalStorage,
                Log,
                Http);

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