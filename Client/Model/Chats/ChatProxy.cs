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
                    if (chat.UserId == ownPeerId)
                    {
                        chat.CssClass = Constants.OwnChatCss;
                        chat.ContainerCssClass = Constants.OwnChatContainerCss;
                        chat.DisplayColor = Constants.OwnColor;
                        chat.SenderName += " (you)";
                    }
                    else
                    {
                        chat.CssClass = Constants.OtherChatCss;
                        chat.ContainerCssClass = Constants.OtherChatContainerCss;
                        chat.DisplayColor = chat.CustomColor;
                    }
                }
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
            log.LogTrace("HIGHLIGHT---> ChatProxy.ReceiveChat(string)");

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
                log.LogDebug($"Received chat with MessageMarkdown '{receivedChat.MessageMarkdown}'");

                if (receivedChat.Key != SecretKey)
                {
                    log.LogError("Received chat with invalid key");
                    return;
                }

                if (receivedChat.UserId == peerId)
                {
                    receivedChat.DisplayColor = Constants.OwnColor;
                    receivedChat.CssClass = Constants.OwnChatCss;
                    receivedChat.ContainerCssClass = Constants.OwnChatContainerCss;
                    receivedChat.SenderName += " (you)";
                }
                else
                {
                    receivedChat.DisplayColor = receivedChat.CustomColor;
                    receivedChat.CssClass = Constants.OtherChatCss;
                    receivedChat.ContainerCssClass = Constants.OtherChatContainerCss;
                }

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
                CssClass = Constants.OwnChatCss,
                ContainerCssClass = Constants.OwnChatContainerCss
            };

            NewChatCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}