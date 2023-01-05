using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model.Chats
{
    public class ChatGuest : SignalRGuestBase
    {
        protected override string SessionKey => "ChatGuestSession";

        public event EventHandler Ding;

        public ChatProxy ChatProxy { get; set; }

        public string SecretKey { get; set; }

        public ChatGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string sessionId,
            SessionHandler session) : base(config, localStorage, log, http, sessionId, session)
        {
            ChatProxy = new ChatProxy(_http, _hostNameFree);
            _log.LogInformation("-> ChatGuest()");
        }

        private async Task ReceiveChats(string receivedJson)
        {
            _log.LogTrace("-> ChatGuest.ReceiveChats(string)");

            if (CurrentSession.Chats == null)
            {
                CurrentSession.Chats = new List<Chat>();
            }

            var chatAdded = await ChatProxy.ReceiveChats(
                RaiseUpdateEvent,
                null, // Do not save messages in the guest, they always need a Host to be online.
                receivedJson,
                CurrentSession.Chats,
                CurrentSession.SessionId,
                PeerInfo.Message.PeerId,
                _log);

            var firstChat = CurrentSession.Chats
                .FirstOrDefault(c => c.SessionName != null);

            if (firstChat != null)
            {
                CurrentSession.SessionName = firstChat.SessionName;
                RaiseUpdateEvent();
            }

            _log.LogTrace("ChatGuest.ReceiveChats(string) ->");

            if (chatAdded)
            {
                Ding?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task LikeChat(string receivedJson)
        {
            _log.LogTrace("-> ChatGuest.LikeChat(string)");

            await ChatProxy.ReceiveLikeChat(
                RaiseUpdateEvent,
                null,
                receivedJson,
                CurrentSession.Chats,
                PeerInfo.Message.PeerId,
                _log);
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> ChatGuest.Connect");

            IsBusy = true;
            IsInError = false;
            IsConnected = false;
            RaiseUpdateEvent();

            var ok = await InitializeSession(_sessionId)
                && await InitializePeerInfo()
                //&& await UnregisterFromPreviousGroup(_unregisterFromGroup)
                && await CreateConnection();

            if (ok)
            {
                _connection.On<string>(Constants.ReceiveChatsMessage, ReceiveChats);
                _connection.On<string>(Constants.LikeChatMessage, LikeChat);

                ok = await StartConnection();

                if (ok)
                {
                    // Ask for existing chats

                    _log.LogTrace("Asking for existing chats");

                    string reasonPhrase = null;

                    var chatsUrl = $"{_hostNameFree}/chats";
                    var httpRequest = new HttpRequestMessage(HttpMethod.Get, chatsUrl);
                    httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

                    var response = await _http.SendAsync(httpRequest);

                    ok = response.IsSuccessStatusCode;

                    if (!ok)
                    {
                        IsConnected = false;
                        IsInError = true;
                        ErrorStatus = "Error";
                        _log.LogError($"Error when asking for existing chats: {reasonPhrase}");
                    }
                    else
                    {
                        IsConnected = true;
                        IsInError = false;
                        Status = "Ready";
                        _log.LogTrace("Done asking for existing chats");
                    }
                }
                else
                {
                    IsConnected = false;
                    IsInError = true;
                    ErrorStatus = "Error";
                    _log.LogError($"Error when starting connection");
                }
            }
            else
            {
                IsConnected = false;
                IsInError = true;
                ErrorStatus = "Error";
                _log.LogError($"Error when connecting");
            }

            IsBusy = false;
            RaiseUpdateEvent();
            _log.LogInformation("ChatGuest.Connect ->");
        }

        public async Task<bool> InitializeSession(string sessionId)
        {
            _log.LogInformation("-> InitializeSession");
            _log.LogDebug($"sessionId: {sessionId}");

            var guestSession = await _session.GetFromStorage(SessionKey, _log);

            if (guestSession != null)
            {
                _unregisterFromGroup = guestSession.SessionId;
            }

            if (guestSession == null
                || guestSession.SessionId != sessionId)
            {
                CurrentSession = new SessionBase
                {
                    SessionId = sessionId,
                    SessionName = Branding.ChatsPageTitle
                };

                await _session.SaveToStorage(CurrentSession, SessionKey, _log);
            }
            else
            {
                CurrentSession = guestSession;
            }

            _log.LogTrace("Session saved to storage");
            _log.LogInformation("InitializeSession ->");
            return true;
        }

        public async Task SaveSessionToStorage()
        {
            await _session.SaveToStorage(CurrentSession, SessionKey, _log);
        }

        public async Task<bool> SendCurrentChat()
        {
            _log.LogTrace("-> SendCurrentChat");
            return await ChatProxy.SendCurrentChat(
                RaiseUpdateEvent,
                PeerInfo.Message,
                CurrentSession.SessionName,
                CurrentSession.SessionId,
                _log);
        }

        public async Task ToggleLikeChat(Chat chat)
        {
            _log.LogDebug("-> ChatGuest.ToggleLikeChat");

            await ChatProxy.ToggleLikeChat(
                chat,
                PeerInfo.Message,
                CurrentSession.SessionId,
                _log);
        }
    }
}