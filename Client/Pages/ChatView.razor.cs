using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Timekeeper.Client.Model.Chats;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class ChatView
    {
        [Parameter]
        public Chat Chat { get; set; }

        [Parameter]
        public ChatHost Host { get; set; }

        [Parameter]
        public ChatGuest Guest { get; set; }

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }

        public async void ToggleLikeChat()
        {
            Log.LogTrace("HIGHLIGHT---> ChatView.ToggleLikeChat");

            Log.LogDebug($"Chat ID: {Chat.UniqueId} / Count: {Chat.Likes.Count}");

            if (Host != null)
            {
                await Host.ToggleLikeChat(Chat);
            }

            if (Guest != null)
            {
                await Guest.ToggleLikeChat(Chat);
            }
        }
    }
}
