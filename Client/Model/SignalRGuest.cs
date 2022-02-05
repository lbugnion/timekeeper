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
    public class SignalRGuest : SignalRGuestBase
    {
        protected override string SessionKey => "GuestSession";

        public SignalRGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string sessionId,
            SessionHandler session) : base(config, localStorage, log, http, sessionId, session)
        {
            _log.LogInformation("-> SignalRGuest()");
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

            IsBusyTEMPO = true;
            IsInErrorTEMPO = false;
            IsConnectedTEMPO = false;

            var ok = await InitializeSession(_sessionId)
                && await InitializePeerInfo()
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
                    _log.LogTrace($"Name is {PeerInfo.Message.DisplayName}");
                    _log.LogTrace($"Sending name {PeerInfo.Message.CustomName}");

                    ok = await AnnounceName();

                    if (ok)
                    {
                        IsConnectedTEMPO = true;
                        IsInErrorTEMPO = false;
                        DisplayMessage("Ready", false);
                    }
                    else
                    {
                        IsConnectedTEMPO = false;
                        IsInErrorTEMPO = true;
                        DisplayMessage("Error", true);
                    }
                }
                else
                {
                    IsConnectedTEMPO = false;
                    IsInErrorTEMPO = true;
                    DisplayMessage("Error", true);
                }
            }
            else
            {
                IsConnectedTEMPO = false;
                IsInErrorTEMPO = true;
                DisplayMessage("Error", true);
            }

            IsBusyTEMPO = false;
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

        public async Task SavePeerInfo()
        {
            await PeerInfo.Save(PeerKey);
        }
    }
}