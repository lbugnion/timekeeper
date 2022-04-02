using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRGuest.Connect");

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
                        IsConnected = true;
                        IsInError = false;
                        DisplayMessage("Ready", false);
                    }
                    else
                    {
                        IsConnected = false;
                        IsInError = true;
                        DisplayMessage("Error", true);
                    }
                }
                else
                {
                    IsConnected = false;
                    IsInError = true;
                    DisplayMessage("Error", true);
                }
            }
            else
            {
                IsConnected = false;
                IsInError = true;
                DisplayMessage("Error", true);
            }

            IsBusy = false;
            RaiseUpdateEvent();
            _log.LogInformation("SignalRGuest.Connect ->");
        }

        public async Task<bool> InitializeSession(string sessionId)
        {
            _log.LogInformation("-> InitializeSession");
            _log.LogDebug($"sessionId: {sessionId}");

            var guestSession = await _session.GetFromStorage(SessionKey, _log);

            if (guestSession == null)
            {
                guestSession = new SessionBase()
                {
                    SessionName = Branding.GuestPageTitle
                };
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