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
        private const string _secretKeyCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*():;.,/?{}[]";
        private string _sessionId;

        public ChatProxy ChatProxy { get; set; }

        public bool IsSessionMismatch
        {
            get;
            private set;
        }

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
            ChatProxy = new ChatProxy(_http, _hostNameFree);
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

        private async Task ReceiveChats(string receivedJson)
        {
            _log.LogTrace("-> ChatHost.ReceiveChats(string)");

            await ChatProxy.ReceiveChats(
                RaiseUpdateEvent,
                SaveSession,
                receivedJson,
                CurrentSession.Chats,
                PeerInfo.Message.PeerId,
                _log);
        }

        private async Task<bool> SendChats()
        {
            return await SendChats(string.Empty);
        }

        private async Task<bool> SendChats(string _)
        {
            // TODO Think about adding paging here for the chats. For instance,
            // send the ones from the last 30 minutes and add a "load more" link on top.

            _log.LogTrace("HIGHLIGHT---> SendChats(string)");

            if (CurrentSession.Chats == null)
            {
                CurrentSession.Chats = new List<Chat>();
            }

            if (CurrentSession.Chats.Count == 0)
            {
                return true;
            }

            return await ChatProxy.SendChats(
                CurrentSession.Chats,
                CurrentSession.SessionName,
                CurrentSession.SessionId,
                _log);
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

            if (ok)
            {
                ChatProxy.UpdateChats(CurrentSession, PeerInfo.Message.PeerId);

                _connection.On<string>(Constants.ReceiveChatsMessage, ReceiveChats);
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

            Status = "Connected";
            IsBusy = false;
            IsInError = false;
            IsConnected = true;
            RaiseUpdateEvent();
            _log.LogInformation("ChatsHost.Connect ->");
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

                // Refresh session
                _log.LogTrace("Refreshing session");

                try
                {
                    var sessions = await _session.GetSessions(_log);
                    var outSession = sessions.FirstOrDefault(s => s.SessionId == CurrentSession.SessionId);
                    CurrentSession = outSession;
                }
                catch (Exception ex)
                {
                    _log.LogError($"Cannot get sessions: {ex.Message}");
                    IsConnected = false;
                    IsInError = true;
                    ErrorStatus = "Error getting sessions";
                    RaiseUpdateEvent();
                    return false;
                }
            }

            RaiseUpdateEvent();

            _log.LogInformation("ChatHost.InitializeSession ->");
            return true;
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
    }
}