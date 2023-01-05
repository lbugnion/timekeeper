using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.Chats;

namespace Timekeeper.Client.Pages
{
    public partial class ManageChatsView : IDisposable
    {
        private ChatHost _handler;
        public const string SendMessageInputId = "chat-text";

        public EditContext CurrentEditContext { get; set; }

        [Parameter]
        public ChatHost Handler
        {
            get => _handler;
            set
            {
                if (value == null)
                {
                    _handler.UpdateUi -= HandlerUpdateUi;
                    _handler.RequestRefresh -= HandlerRequestRefresh;
                    _handler.Ding -= HandlerDing;
                }

                _handler = value;

                if (_handler != null)
                {
                    _handler.UpdateUi += HandlerUpdateUi;
                    _handler.RequestRefresh += HandlerRequestRefresh;
                    _handler.Ding += HandlerDing;
                }
            }
        }

        private async void HandlerDing(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("sound.ding");
        }

        private async void HandlerRequestRefresh(object sender, EventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("host.refreshPage");
        }

        [Parameter]
        public ManageChats Parent { get; set; }

        public string SessionName
        {
            get
            {
                if (Handler.CurrentSession == null)
                {
                    return "No session";
                }

                return Handler.CurrentSession.SessionName;
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

        public string WindowTitle
        {
            get
            {
                if (Handler == null
                    || Handler.CurrentSession == null
                    || string.IsNullOrEmpty(Handler.CurrentSession.SessionName))
                {
                    return Branding.ChatsPageTitle;
                }

                return $"{Handler.CurrentSession.SessionName} {Branding.ChatsPageTitle}";
            }
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

        protected override async Task OnInitializedAsync()
        {
            Log.LogTrace("-> OnInitializedAsync");

            Handler.ChatProxy.NewChatCreated += ChatProxyNewChatCreated;

            if (Handler.ChatProxy.NewChat != null)
            {
                CurrentEditContext = new EditContext(Handler.ChatProxy.NewChat);
            }

            await JSRuntime.InvokeVoidAsync("branding.setTitle", WindowTitle);
        }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
                Handler.RequestRefresh -= HandlerRequestRefresh;
                Handler.Ding -= HandlerDing;
            }

            if (Handler.ChatProxy != null)
            {
                Handler.ChatProxy.NewChatCreated -= ChatProxyNewChatCreated;
            }
        }

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }

        public async void HandleFocus()
        {
            await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
        }

        public async void HandleKeyPress(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                if (!args.ShiftKey)
                {
                    await Handler.SendCurrentChat();
                    await JSRuntime.InvokeVoidAsync("host.focusAndSelect", SendMessageInputId);
                }
            }
        }

        public void UseSecretKey(int use)
        {
            // TODO Restore for V0.8.1
            //if (use == 0)
            //{
            //    Handler.SecretKey = null;
            //}
            //else
            //{
            //    Handler.RegenerateSecretKey();
            //}
        }
    }
}