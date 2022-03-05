using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Timekeeper.Client.Model.Chats;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class ManageChatsView : IDisposable
    {
        private ChatHost _handler;

        [Parameter]
        public ManageChats Parent { get; set; }

        [Parameter]
        public ChatHost Handler
        {
            get => _handler;
            set
            {
                if (value == null)
                {
                    _handler.UpdateUi -= HandlerUpdateUi;
                }

                _handler = value;

                if (_handler != null)
                {
                    _handler.UpdateUi += HandlerUpdateUi;
                }
            }
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

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

        public EditContext CurrentEditContext { get; set; }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }

            if (Handler.ChatProxy != null)
            {
                Handler.ChatProxy.NewChatCreated -= ChatProxyNewChatCreated;
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

        protected override void OnInitialized()
        {
            Log.LogTrace("-> OnInitialized");

            Handler.ChatProxy.NewChatCreated += ChatProxyNewChatCreated;

            if (Handler.ChatProxy.NewChat != null)
            {
                CurrentEditContext = new EditContext(Handler.ChatProxy.NewChat);
            }

#if OFFLINE
            Handler.CurrentSession.Chats.Clear();

            for (var index = 0; index < 25; index++)
            {
                var chat = new Chat
                {
                    MessageMarkdown = "This is a test message. This is a test message. This is a test message. This is a test message",
                    SessionName = Handler.CurrentSession.SessionName
                };

                if (index % 2 == 0)
                {
                    chat.Color = Constants.OwnColor;
                    chat.CssClass = Constants.OwnChatCss;
                    chat.ContainerCssClass = Constants.OwnChatContainerCss;
                    chat.SenderName = "Laurent";
                }
                else
                {
                    chat.Color = Handler.ChatColorToOthers;
                    chat.CssClass = Constants.OtherChatCss;
                    chat.ContainerCssClass = Constants.OtherChatContainerCss;
                    chat.SenderName = "Jason";
                }

                Handler.CurrentSession.Chats.Add(chat);
            }
#endif
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

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }
    }
}
