using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
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
    public class ChatGuest : SignalRGuestBase
    {
        protected override string SessionKey => "ChatGuestSession";

        public Chat NewChat { get; set; } = new Chat();

        public string SecretKey { get; set; }

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

            //await ReceiveChat(receivedChat);
        }

        public ChatGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string sessionId,
            SessionHandler session) : base(config, localStorage, log, http, sessionId, session)
        {
            _log.LogInformation("-> ChatGuest()");
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
                && await UnregisterFromPreviousGroup(_unregisterFromGroup)
                && await CreateConnection();

            if (ok)
            {
#if !OFFLINE
                //_connection.On<string>(Constants.ReceiveChatsMessage, ReceiveAllChats);

                ok = await StartConnection();
#endif

                if (ok)
                {
                    // Ask for existing chats

                    _log.LogTrace("Asking for existing chats");

                    string reasonPhrase = null;

#if !OFFLINE
                    var chatsUrl = $"{_hostNameFree}/chats";
                    var httpRequest = new HttpRequestMessage(HttpMethod.Get, chatsUrl);
                    httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

                    var response = await _http.SendAsync(httpRequest);

                    ok = response.IsSuccessStatusCode;
#endif

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
