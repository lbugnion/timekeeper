using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class SignalRGuest : SignalRHandler
    {
        private string _sessionId;

        private string _unregisterFromGroup = null;

        protected override string SessionKey => "GuestSession";

        public SignalRGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string sessionId,
            SessionHandler session) : base(config, localStorage, log, http, session)
        {
            _log.LogInformation("> SignalRGuest()");
            _sessionId = sessionId;
        }

        public async Task<bool> AnnounceName()
        {
            _log.LogInformation($"-> {nameof(AnnounceName)}");
            _log.LogDebug($"GuestId: {PeerInfo.Message.PeerId}");

            var json = JsonConvert.SerializeObject(PeerInfo.Message);
            _log.LogDebug($"json: {json}");

            return await AnnounceNameJson(json);
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRGuest.Connect");

            IsBusy = true;

            var ok = await InitializeSession(_sessionId)
                && await InitializeGuestInfo()
                && await UnregisterFromPreviousGroup(_unregisterFromGroup)
                && await CreateConnection();

            if (ok)
            {
                _connection.On<string>(Constants.HostToPeerMessageName, DisplayReceivedMessage);
                _connection.On(Constants.HostToPeerRequestAnnounceMessageName, AnnounceName);
                _connection.On<string>(Constants.StartClockMessageName, s => ReceiveStartClock(s, false));
                _connection.On<string>(Constants.StopClockMessage, s => StopLocalClock(s, false));

                ok = await StartConnection();

                if (ok)
                {
                    IsConnected = true;
                    DisplayMessage("Ready", false);

                    _log.LogTrace($"Name is {PeerInfo.Message.DisplayName}");
                    _log.LogTrace($"Sending name {PeerInfo.Message.CustomName}");

                    ok = await AnnounceName();

                    if (!ok)
                    {
                        IsConnected = false;
                        DisplayMessage("Error", true);
                    }
                }
                else
                {
                    IsConnected = false;
                    DisplayMessage("Error", true);
                }
            }
            else
            {
                IsConnected = false;
                DisplayMessage("Error", true);
            }

            IsBusy = false;
            _log.LogInformation("SignalRGuest.Connect ->");
        }

        public async Task<bool> InitializeSession(string sessionId)
        {
            _log.LogInformation("-> InitializeSession");
            _log.LogDebug($"sessionId: {sessionId}");

            var guestSession = await _session.GetFromStorage(SessionKey, _log);

            if (guestSession == null)
            {
                guestSession = new SessionBase();
            }
            else
            {
                _unregisterFromGroup = guestSession.SessionId;
            }

            guestSession.SessionId = sessionId;
            guestSession.Clocks = new List<Clock>(); // Always reset the clocks

            CurrentSession = guestSession;
            await _session.SaveToStorage(CurrentSession, SessionKey, _log);
            _log.LogTrace("Session saved to storage");

            _log.LogInformation("InitializeSession ->");
            return true;
        }
    }
}