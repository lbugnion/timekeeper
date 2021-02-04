using TimekeeperClient.Model;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace TimekeeperClient.Pages
{
    public partial class Host : IDisposable
    {
        public SignalRHost Handler
        {
            get;
            private set;
        }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi += HandlerUpdateUi;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            BackgroundClassName = Index.NormalBackgroundClassName;

            Handler = new SignalRHost(
                Config,
                Log,
                Http);

            Handler.UpdateUi += HandlerUpdateUi;
            await Handler.Connect();
        }

        public string BackgroundClassName
        {
            get;
            private set;
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            Log.LogTrace("HandlerUpdateUi");

            if (Handler.IsRed)
            {
                BackgroundClassName = Index.RedBackgroundClassName;
            }
            else
            {
                if (Handler.IsYellow)
                {
                    BackgroundClassName = Index.YellowBackgroundClassName;
                }
            }

            StateHasChanged();
        }
    }
}
