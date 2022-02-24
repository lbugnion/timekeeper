using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model.Chats
{
    public class ChatHost : SignalRHostBase
    {
#if OFFLINE
        public const bool IsDebugOffline = true;
#else
        public const bool IsDebugOffline = false;
#endif

        private string _sessionId;

        public ChatHost(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            NavigationManager nav,
            SessionHandler session,
            string sessionId) : base(config, localStorage, log, http, nav, session)
        {
            _sessionId = sessionId;
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> ChatHost.Connect");

            IsBusy = true;
            IsInError = false;
            IsConnected = false;
            RaiseUpdateEvent();

            var ok = await InitializeSession(_sessionId);

            if (!ok)
            {
                // Error cases are handled in InitializeSession
                return;
            }

            ok = await InitializePeerInfo()
                && await CreateConnection();

#if !OFFLINE
            if (ok)
            {
                _connection.On<string>(Constants.ChatMessage, c => ReceiveChat(c));
                _connection.On<string>(Constants.RequestChatsMessage, SendChats);

                ok = await StartConnection();
            }

            if (!ok)
            {
                _log.LogTrace("StartConnection NOT OK");
                IsConnected = false;
                IsInError = true;
                IsBusy = false;
                ErrorStatus = "Error";
                RaiseUpdateEvent();
                return;
            }

            ok = await SendChats();

            if (!ok)
            {
                _log.LogTrace("Error when sending chats");
                IsConnected = false;
                IsInError = true;
                IsBusy = false;
                ErrorStatus = "Error sending chats";
                RaiseUpdateEvent();
                return;
            }
#endif

            Status = "Connected";
            IsBusy = false;
            IsInError = false;
            IsConnected = true;
            RaiseUpdateEvent();
            _log.LogInformation("ChatsHost.Connect ->");
        }

        private const string _secretKeyCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*():;.,/?{}[]";

        public void RegenerateSecretKey()
        {
            var random = new Random();
            var secretKeyLength = random.Next(10, 15);
            var secretKey = new StringBuilder();

            for (var index = 0; index < secretKeyLength; index++)
            {
                var characterIndex = random.Next(0, _secretKeyCharacters.Length);
                secretKey.Append(_secretKeyCharacters[characterIndex]);
            }

            CurrentSession.SecretKey = secretKey.ToString();

            // TODO Save key in session
            // TODO Broadcast to other hosts.
            // TODO Warn host to let users know about the change or they will be excluded from the chat

            RaiseUpdateEvent();
        }

        public Chat NewChat { get; set; } = new Chat();

        private async Task<bool> SendChats()
        {
            return await SendChats(string.Empty);
        }

        private async Task<bool> SendChats(string _)
        {
            _log.LogTrace("-> SendChats");

            _log.LogDebug($"HIGHLIGHT--CurrentSession is null: {CurrentSession == null}");
            _log.LogDebug($"HIGHLIGHT--Chats is null: {CurrentSession.Chats == null}");

            if (CurrentSession.Chats == null)
            {
                CurrentSession.Chats = new List<Chat>();
            }

            _log.LogDebug($"HIGHLIGHT--Chats count is zero: {CurrentSession.Chats.Count == 0}");

            if (CurrentSession.Chats.Count == 0)
            {
                return true;
            }

            foreach (var chat in CurrentSession.Chats)
            {
                chat.SessionName = null;
            }

            NewChat.SessionName = CurrentSession.SessionName;

            var list = new ListOfChats();
            list.Chats.AddRange(CurrentSession.Chats);

            var json = JsonConvert.SerializeObject(list);

            _log.LogDebug($"json: {json}");

            var chatsUrl = $"{_hostName}/chats";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, chatsUrl);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
            httpRequest.Content = new StringContent(json);

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        private async Task ReceiveChat(string chatJson)
        {
            Chat receivedChat;

            try
            {
                receivedChat = JsonConvert.DeserializeObject<Chat>(chatJson);
            }
            catch
            {
                _log.LogTrace("Error with received chat");
                return;
            }

            await ReceiveChat(receivedChat);
        }

        public string SecretKey { get; set; }

        private async Task ReceiveChat(Chat receivedChat)
        {
            if (receivedChat.Key != SecretKey)
            {
                _log.LogError("Received chat with invalid key");
                return;
            }

            if (receivedChat.UserId == PeerInfo.Message.PeerId)
            {
                receivedChat.Color = Constants.OwnColor;
                receivedChat.CssClass = Constants.OwnChatCss;
                receivedChat.ContainerCssClass = Constants.OwnChatContainerCss;
            }
            else
            {
                receivedChat.CssClass = Constants.OtherChatCss;
                receivedChat.ContainerCssClass = Constants.OtherChatContainerCss;
            }

            CurrentSession.Chats.Insert(0, receivedChat);

            await SaveSessionToStorage();
            RaiseUpdateEvent();
        }

        public async Task SaveSessionToStorage()
        {
            await _session.SaveToStorage(CurrentSession, SessionKey, _log);
        }

        private async Task<bool> DoPublishUnpublishPoll(Poll poll, bool mustPublish, bool? mustOpen = null)
        {
            Status = "Attempting to publish poll";
            RaiseUpdateEvent();
            string publishUnpublishUrl;

            if (mustPublish)
            {
                publishUnpublishUrl = $"{_hostName}/publish-poll";
            }
            else
            {
                publishUnpublishUrl = $"{_hostName}/unpublish-poll";
            }

            _log.LogDebug($"publishUnpublishUrl: {publishUnpublishUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, publishUnpublishUrl);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

            string json;

            if (poll.IsVotingOpen)
            {
                json = JsonConvert.SerializeObject(poll.GetSafeCopy(), Formatting.Indented);
            }
            else
            {
                json = JsonConvert.SerializeObject(poll, Formatting.Indented);
            }

            httpRequest.Content = new StringContent(json);

            poll.IsBroadcasting = true;
            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Cannot send message: {response.ReasonPhrase}");
                poll.IsBroadcasting = false;
                ErrorStatus = "Error publishing poll";
            }
            else
            {
                if (mustPublish)
                {
                    Status = "Poll published";
                }
                else
                {
                    Status = "Poll unpublished";
                }

                poll.IsPublished = mustPublish;

                if (mustOpen != null)
                {
                    poll.IsVotingOpen = mustOpen.Value;
                }

                await _session.Save(CurrentSession, SessionKey, _log);
            }

            RaiseUpdateEvent();
            poll.IsBroadcasting = false;
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PublishUnpublishPoll(Poll poll, bool mustPublish)
        {
            _log.LogTrace($"-> {nameof(PublishUnpublishPoll)} {mustPublish}");

            if (poll == null
                || poll.IsPublished && mustPublish
                || !poll.IsPublished && !mustPublish)
            {
                _log.LogTrace($"Poll is already in the correct state");
                return false;
            }

            poll.IsVotingOpen = mustPublish;
            poll.SessionName = CurrentSession.SessionName;

            var success = await DoPublishUnpublishPoll(poll, mustPublish);

            _log.LogTrace($"{nameof(PublishUnpublishPoll)} ->");
            return success;
        }

        public async Task<bool> InitializeSession(string sessionId)
        {
            _log.LogInformation("-> ChatHost.InitializeSession");

            CurrentSession = await _session.GetFromStorage(SessionKey, _log);

            if (CurrentSession == null)
            {
                _log.LogWarning("Session in storage is Null");
                IsBusy = false;
                IsInError = false;
                IsConnected = false;
                _nav.NavigateTo("/");
                return false;
            }
            else
            {
                _log.LogDebug($"SessionId in Storage: {CurrentSession.SessionId}");

                if (CurrentSession.SessionId != sessionId)
                {
                    _log.LogTrace("Session ID mismatch");
                    CurrentSession = null;
                    ErrorStatus = "Session ID mismatch";
                    IsBusy = false;
                    IsInError = false;
                    IsConnected = false;
                    IsSessionMismatch = true;
                    RaiseUpdateEvent();
                    _log.LogTrace("Done informing user");
                    return false;
                }

#if !OFFLINE
                // Refresh session
                var sessions = await _session.GetSessions(_log);
                var outSession = sessions.FirstOrDefault(s => s.SessionId == CurrentSession.SessionId);

                _log.LogDebug($"outSession == null: {outSession == null}");

                CurrentSession = outSession;
#endif
            }

            RaiseUpdateEvent();

            _log.LogInformation("ChatHost.InitializeSession ->");
            return true;
        }

        public bool IsSessionMismatch
        {
            get;
            private set;
        }

#if OFFLINE
        private int _chatCounter;
        private readonly string _otherUserId = Guid.NewGuid().ToString();
#endif

        public async Task AddDebugChat()
        {
#if OFFLINE
            var chat = new Chat
            {
                Color = ChatColorToOthers,
                MessageDateTime = DateTime.Now,
                SenderName = _chatCounter % 2 == 0 ? "Laurent" : "Vanch",
                MessageMarkdown = "This is a *test message*",
                UserId = _chatCounter % 2 == 0 ? PeerInfo.Message.PeerId : _otherUserId,
            };

            var json = JsonConvert.SerializeObject(chat);

            await ReceiveChat(json);
            _chatCounter++;
#endif
        }
    }
}
