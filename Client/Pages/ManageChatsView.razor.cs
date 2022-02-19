using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
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

        public Chat NewChat { get; set; }

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
        }

        protected override void OnAfterRender(bool firstRender)
        {
            NewChat = new Chat();
            CurrentEditContext = new EditContext(NewChat);
        }
    }
}
