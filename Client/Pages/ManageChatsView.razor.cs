using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
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
            CurrentEditContext = new EditContext(Handler.NewChat);

            var random = new Random();

            var myColor = $"#{random.Next(128, 255).ToString("X2")}{random.Next(128, 255).ToString("X2")}{random.Next(128, 255).ToString("X2")}";
            var otherColor = $"#{random.Next(128, 255).ToString("X2")}{random.Next(128, 255).ToString("X2")}{random.Next(128, 255).ToString("X2")}";

            for (var index = 0; index < 25; index++)
            {
                var chat = new Chat
                {
                    MessageMarkdown = "This is a test message. This is a test message. This is a test message. This is a test message",
                    SessionName = Handler.CurrentSession.SessionName
                };

                if (index % 2 == 0)
                {
                    chat.Color = myColor;
                    chat.CssClass = "own-chat";
                    chat.ContainerCssClass = "own-chat-container";
                    chat.SenderName = "Laurent";
                }
                else
                {
                    chat.Color = otherColor;
                    chat.CssClass = "other-chat";
                    chat.ContainerCssClass = "other-chat-container";
                    chat.SenderName = "Jason";
                }

                Handler.CurrentSession.Chats.Add(chat);
            }
        }

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }
    }
}
