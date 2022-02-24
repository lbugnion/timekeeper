using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model.Chats;

namespace Timekeeper.Client.Pages
{
    public partial class DisplayChats
    {
        public const string VisibilityVisible = "visible";
        public const string VisibilityInvisible = "invisible";

        public string UiVisibility
        {
            get;
            set;
        }

        public void ToggleFocus()
        {
            Log.LogTrace("-> ToggleFocus");

            if (UiVisibility == VisibilityVisible)
            {
                Log.LogTrace("Setting Invisible");
                UiVisibility = VisibilityInvisible;
            }
            else
            {
                Log.LogTrace("Setting Visible");
                UiVisibility = VisibilityVisible;
            }
        }

        public EditContext CurrentEditContext { get; set; }

        public ChatGuest Handler
        {
            get;
            private set;
        }

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }

        public bool ShowNoSessionMessage
        {
            get;
            private set;
        }

        [Parameter]
        public string SessionId
        {
            get;
            set;
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
                    Handler = new ChatGuest(
                        Config,
                        LocalStorage,
                        Log,
                        Http,
                        SessionId,
                        Session);

                    CurrentEditContext = new EditContext(Handler.NewChat);

                    Handler.UpdateUi += HandlerUpdateUi;
                    await Handler.Connect();

#if OFFLINE
                    Handler.CurrentSession?.Chats?.Clear();
#endif
                }
            }

            UiVisibility = VisibilityVisible;
            Log.LogInformation("OnInitializedAsync ->");
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
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
    }
}
