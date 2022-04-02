using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.Chats;

namespace Timekeeper.Client.Pages
{
    public partial class DisplayChats : IDisposable
    {
        public const string VisibilityVisible = "visible";
        public const string VisibilityInvisible = "invisible";
        public const string SendMessageInputId = "chat-text";

        public string WindowTitle
        {
            get
            {
                if (Handler == null
                    || Handler.CurrentSession == null
                    || string.IsNullOrEmpty(Handler.CurrentSession.SessionName)
                    || Handler.CurrentSession.SessionName == Branding.ChatsPageTitle)
                {
                    return Branding.ChatsPageTitle;
                }

                return $"{Handler.CurrentSession.SessionName} {Branding.ChatsPageTitle}";
            }
        }

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

        public string UserName
        {
            get
            {
                return Handler.PeerInfo.Message.DisplayName;
            }

            set
            {
                Handler.SetCustomUserName(value).Wait();
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

                    Handler.ChatProxy.SetNewChat();

                    Handler.ChatProxy.NewChatCreated += ChatProxyNewChatCreated;

                    if (Handler.ChatProxy.NewChat != null)
                    {
                        CurrentEditContext = new EditContext(Handler.ChatProxy.NewChat);
                    }

                    //await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);

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

        private void ChatProxyNewChatCreated(object sender, EventArgs e)
        {
            if (Handler.ChatProxy.NewChat == null)
            {
                CurrentEditContext = null;
                return;
            }

            CurrentEditContext = new EditContext(Handler.ChatProxy.NewChat);
        }

        private async void HandlerUpdateUi(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);
            StateHasChanged();
        }

        public async void Dispose()
        {
            Log.LogTrace("Dispose");

            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }

            if (Handler.ChatProxy != null)
            {
                Handler.ChatProxy.NewChatCreated -= ChatProxyNewChatCreated;
            }

            await Task.Run(async () =>
            {
                await Handler.Disconnect();
            });
        }

        public async void HandleFocus()
        {
            await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
        }

        public async void HandleKeyPress(KeyboardEventArgs args)
        {
            if (args.CtrlKey)
            {
                await Handler.SendCurrentChat();
                await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
            }
        }
    }
}
