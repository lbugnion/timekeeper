using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model.Chats
{
    public class ChatProxy
    {
        public event EventHandler NewChatCreated;

        private string _hostNameFree;
        private HttpClient _http;

        public string ErrorStatus { get; private set; }

        public bool IsInError { get; private set; }

        public bool IsSendingChat { get; set; }

        public Chat NewChat { get; set; }

        public string SecretKey { get; set; }

        public string Status { get; private set; }

        public ChatProxy(
                    HttpClient http,
            string hostNameFree)
        {
            _http = http;
            _hostNameFree = hostNameFree;
        }

        public static void UpdateChats(
            SessionBase currentSession,
            string ownPeerId)
        {
            if (currentSession.Chats != null)
            {
                foreach (var chat in currentSession.Chats)
                {
                    UpdateChatStyles(chat, ownPeerId);
                }
            }
        }

        private static void UpdateChatStyles(Chat chat, string ownPeerId)
        {
            if (chat.UserId == ownPeerId)
            {
                chat.CssClass = Constants.OwnChatCss;
                chat.ButtonLikeCssClass = Constants.HideLikeCss;
                chat.SpanLikeCssClass = Constants.ShowLikeCss;
                chat.DisplayColor = Constants.OwnColor;

                if (chat.Suffix != Constants.You)
                {
                    chat.Suffix = Constants.You;
                }
            }
            else
            {
                chat.CssClass = Constants.OthersChatCss;
                chat.ButtonLikeCssClass = Constants.ShowLikeCss;
                chat.SpanLikeCssClass = Constants.HideLikeCss;
                chat.DisplayColor = chat.CustomColor;
                chat.Suffix = string.Empty;
            }

            if (chat.Likes.Count == 0)
            {
                chat.LikeThumbCssClass = Constants.InactiveThumbCssClass;
                chat.BackgroundLikeCssClass = Constants.NeutralLikeContainer;
            }
            else if (chat.Likes.Any(l => l.PeerId == ownPeerId))
            {
                chat.LikeThumbCssClass = Constants.ActiveOwnThumbCssClass;
                chat.BackgroundLikeCssClass = Constants.ActiveLikeContainer;
            }
            else
            {
                chat.LikeThumbCssClass = Constants.ActiveOthersThumbCssClass;
                chat.BackgroundLikeCssClass = Constants.NeutralLikeContainer;
            }
        }

        public async Task LikeChat(
            Action raiseUpdateEvent,
            Func<Task> saveChats,
            string receivedJson,
            IList<Chat> allChats,
            string peerId,
            ILogger log)
        {
            log.LogTrace("-> ChatProxy.LikeChat(string)");

            LikeChatMessage receivedMessage;

            try
            {
                receivedMessage = JsonConvert.DeserializeObject<LikeChatMessage>(receivedJson);
            }
            catch
            {
                log.LogTrace("Error with received like message");
                return;
            }

            await LikeChat(
                raiseUpdateEvent,
                saveChats,
                receivedMessage,
                allChats,
                peerId,
                log);
        }

        public async Task LikeChat(
            Action raiseUpdateEvent,
            Func<Task> saveChats,
            LikeChatMessage receivedMessage,
            IList<Chat> allChats,
            string peerId,
            ILogger log)
        {
            log.LogTrace("HIGHLIGHT---> ChatProxy.ReceiveChat(ListOfChats)");

            if (receivedMessage.Key != SecretKey)
            {
                log.LogError("Received message with invalid key");
                return;
            }

            var likedChat = allChats.FirstOrDefault(c => c.UniqueId == receivedMessage.MessageId);

            if (likedChat != null)
            {
                var like = likedChat.Likes
                    .FirstOrDefault(l => l.PeerId == receivedMessage.Peer.PeerId);

                if (receivedMessage.IsLiked)
                {
                    if (like == null)
                    {
                        var customName = receivedMessage.Peer.CustomName;

                        if (receivedMessage.Peer.PeerId == peerId)
                        {
                            customName = Constants.YouName;
                        }

                        likedChat.Likes.Add(new PeerMessage
                        {
                            CustomName = customName,
                            PeerId = receivedMessage.Peer.PeerId
                        });
                    }
                }
                else
                {
                    if (like != null)
                    {
                        likedChat.Likes.Remove(like);
                    }
                }
            }

            UpdateChatStyles(likedChat, peerId);

            if (raiseUpdateEvent != null)
            {
                raiseUpdateEvent();
            }

            if (saveChats != null)
            {
                await saveChats();
            }
        }

        public async Task ReceiveChats(
            Action raiseUpdateEvent,
            Func<Task> saveChats,
            string receivedChatJson,
            IList<Chat> allChats,
            string peerId,
            ILogger log)
        {
            log.LogTrace("-> ChatProxy.ReceiveChat(string)");

            ListOfChats receivedChats;

            try
            {
                receivedChats = JsonConvert.DeserializeObject<ListOfChats>(receivedChatJson);
            }
            catch
            {
                log.LogTrace("Error with received chat");
                return;
            }

            await ReceiveChats(
                raiseUpdateEvent,
                saveChats,
                receivedChats,
                allChats,
                peerId,
                log);
        }

        public async Task ReceiveChats(
            Action raiseUpdateEvent,
            Func<Task> saveChats,
            ListOfChats receivedChats,
            IList<Chat> allChats,
            string peerId,
            ILogger log)
        {
            log.LogTrace("HIGHLIGHT---> ChatProxy.ReceiveChat(ListOfChats)");

            foreach (var receivedChat in receivedChats.Chats.OrderBy(c => c.MessageDateTime))
            {
                log.LogDebug($"Received chat with MessageMarkdown '{receivedChat.MessageMarkdown}' and ID {receivedChat.UniqueId}");

                if (receivedChat.Key != SecretKey)
                {
                    log.LogError("Received chat with invalid key");
                    return;
                }

                UpdateChatStyles(receivedChat, peerId);

                if (!string.IsNullOrEmpty(receivedChat.SessionName))
                {
                    foreach (var chat in allChats.Where(c => c.SessionName != null))
                    {
                        chat.SessionName = null;
                    }
                }

                if (!allChats.Any(c => c.UniqueId == receivedChat.UniqueId))
                {
                    var nextChat = allChats
                        .FirstOrDefault(c => receivedChat.MessageDateTime > c.MessageDateTime);

                    if (nextChat == null)
                    {
                        allChats.Insert(0, receivedChat);
                    }
                    else
                    {
                        var index = allChats.IndexOf(nextChat);
                        allChats.Insert(index, receivedChat);
                    }
                }

                if (!string.IsNullOrEmpty(receivedChat.SessionName))
                {
                    var firstChat = allChats.FirstOrDefault();
                    if (firstChat != null)
                    {
                        firstChat.SessionName = receivedChat.SessionName;
                    }
                }
            }

            if (raiseUpdateEvent != null)
            {
                raiseUpdateEvent();
            }

            if (saveChats != null)
            {
                await saveChats();
            }
        }

        public async Task<bool> SendChats(
            IList<Chat> chats,
            string sessionName,
            string sessionId,
            ILogger log)
        {
            if (chats.Count == 0)
            {
                return true;
            }

            foreach (var chat in chats)
            {
                chat.SessionName = null;
            }

            chats.First().SessionName = sessionName;

            var list = new ListOfChats();
            list.Chats.AddRange(chats);

            var json = JsonConvert.SerializeObject(list);

            //_log.LogDebug($"json: {json}");

            var chatsUrl = $"{_hostNameFree}/chats";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, chatsUrl);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, sessionId);
            httpRequest.Content = new StringContent(json);

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                IsInError = true;
                log.LogError($"Issue sending chat: {response.StatusCode} / {response.ReasonPhrase}");
                ErrorStatus = "Error connecting to Chat service, try to refresh the page and send again";
                return false;
            }

            IsInError = false;
            ErrorStatus = null;
            Status = "Chat sent";
            return true;
        }

        public async Task<bool> SendCurrentChat(
            Action raiseUpdateEvent,
            PeerMessage peerInfoMessage,
            string sessionName,
            string sessionId,
            ILogger log)
        {
            if (string.IsNullOrEmpty(NewChat.MessageMarkdown))
            {
                return true;
            }

            IsSendingChat = true;
            raiseUpdateEvent();

            NewChat.UserId = peerInfoMessage.PeerId;
            NewChat.SenderName = peerInfoMessage.DisplayName;
            NewChat.MessageDateTime = DateTime.Now;
            NewChat.CustomColor = peerInfoMessage.ChatColor;

            var words = NewChat.MessageMarkdown.Split(' ');

            for (var index = 0; index < words.Length; index++)
            {
                if (words[index].StartsWith("http://")
                    || words[index].StartsWith("https://"))
                {
                    words[index] = $"[{words[index]}]({words[index]})";
                }
            }

            NewChat.MessageMarkdown = string.Join(' ', words);

            var ok = await SendChats(
                new List<Chat>
                {
                    NewChat
                },
                sessionName,
                sessionId,
                log);

            SetNewChat();

            IsSendingChat = false;
            raiseUpdateEvent();

            return ok;
        }

        public void SetNewChat()
        {
            NewChat = new Chat()
            {
                UniqueId = Guid.NewGuid().ToString(),
            };

            NewChatCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}