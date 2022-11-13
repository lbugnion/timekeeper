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
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class SignalRHost : SignalRHostBase
    {
        public const string StartAllClocksText = "Start all clocks";
        public const string StartSelectedClocksText = "Start selected clocks";

        public int AnonymousGuests
        {
            get;
            private set;
        }

        public int AnonymousHosts
        {
            get;
            private set;
        }

        public int ConnectedGuests
        {
            get
            {
                return AnonymousGuests + (NamedGuests == null ? 0 : NamedGuests.Count);
            }
        }

        public int ConnectedHosts
        {
            get
            {
                return AnonymousHosts + (NamedHosts == null ? 0 : NamedHosts.Count);
            }
        }

        private string _sessionId;

        public IList<PeerMessage> ConnectedPeers
        {
            get;
            private set;
        }

        public string InputMessage
        {
            get;
            set;
        }

        public bool IsDeleteSessionWarningVisible
        {
            get;
            private set;
        }

        public bool IsModifySessionDisabled
        {
            get;
            private set;
        }

        public bool IsSendMessageDisabled
        {
            get;
            private set;
        }

        public IList<PeerMessage> NamedGuests
        {
            get;
            private set;
        }

        public IList<PeerMessage> NamedHosts
        {
            get;
            private set;
        }

        public string StartClocksButtonText
        {
            get;
            private set;
        }

        public SignalRHost(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            NavigationManager nav,
            SessionHandler session,
            string sessionId = null) : base(config, localStorage, log, http, nav, session)
        {
            _sessionId = sessionId;
            ConnectedPeers = new List<PeerMessage>();
            StartClocksButtonText = StartAllClocksText;
        }

        private void ClockSelectionChanged(object sender, bool e)
        {
            _log.LogInformation("-> ClockSelectionChanged");

            if (CurrentSession.Clocks.Any(c => c.IsSelected))
            {
                StartClocksButtonText = StartSelectedClocksText;
            }
            else
            {
                StartClocksButtonText = StartAllClocksText;
            }

            RaiseUpdateEvent();
        }

        private async Task<bool> RequestAnnounce()
        {
            _log.LogInformation($"-> {nameof(RequestAnnounce)}");

            var announceUrl = $"{_hostName}/request-announce";
            _log.LogDebug($"announceUrl: {announceUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, announceUrl);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Cannot request guests to announce themselves: {response.ReasonPhrase}");
                _log.LogInformation($"{nameof(RequestAnnounce)} ->");
                return false;
            }

            return true;
        }

        private void UpdateConnectedPeers(PeerMessage message)
        {
            if (message != null)
            {
                ConnectedPeers.Add(message);
            }

            NamedGuests = ConnectedPeers
                .Where(g => !string.IsNullOrEmpty(g.CustomName)
                    && g.CustomName != PeerMessage.AnonymousName
                    && !g.IsHost).ToList();

            AnonymousGuests = ConnectedPeers
                .Count(g => (string.IsNullOrEmpty(g.CustomName)
                    || g.CustomName == PeerMessage.AnonymousName)
                    && !g.IsHost);

            NamedHosts = ConnectedPeers
                .Where(g => !string.IsNullOrEmpty(g.CustomName)
                    && g.CustomName != PeerMessage.AnonymousName
                    && g.IsHost).ToList();

            AnonymousHosts = ConnectedPeers
                .Count(g => (string.IsNullOrEmpty(g.CustomName)
                    || g.CustomName == PeerMessage.AnonymousName)
                    && g.IsHost);

            RaiseUpdateEvent();

            foreach (var guest in ConnectedPeers)
            {
                _log.LogDebug($"{guest.CustomName}");
                _log.LogDebug($"{guest.DisplayName}");
            }
        }

        private async Task UpdateLocalHost(string json)
        {
            _log.LogInformation("-> SignalRHost.UpdateLocalHost");

            try
            {
                var info = JsonConvert.DeserializeObject<UpdateHostInfo>(json);

                _log.LogDebug($"Action: {info.Action}");

                switch (info.Action)
                {
                    case UpdateAction.UpdateSessionName:
                        if (!string.IsNullOrEmpty(info.SessionName))
                        {
                            CurrentSession.SessionName = info.SessionName;
                            await _session.SaveToStorage(CurrentSession, SessionKey, _log);
                            RaiseUpdateEvent();
                        }
                        return;

                    case UpdateAction.AddClock:
                        if (info.Clock != null
                            && info.PreviousClockId != null)
                        {
                            var existingClock = CurrentSession.Clocks
                                .FirstOrDefault(c => c.Message.ClockId == info.Clock.ClockId);

                            if (existingClock != null)
                            {
                                return;
                            }

                            var previousClock = CurrentSession.Clocks
                                .FirstOrDefault(c => c.Message.ClockId == info.PreviousClockId);

                            if (previousClock == null)
                            {
                                _log.LogWarning($"No clocks found for ID {info.PreviousClockId}");
                                return;
                            }

                            await AddClockAfter(previousClock, info.Clock);
                            await _session.SaveToStorage(CurrentSession, SessionKey, _log);
                            RaiseUpdateEvent();
                        }
                        break;

                    case UpdateAction.DeleteClock:
                        if (info.Clock != null)
                        {
                            var existingClock = CurrentSession.Clocks
                                .FirstOrDefault(c => c.Message.ClockId == info.Clock.ClockId);

                            if (existingClock != null)
                            {
                                existingClock.Message.WasDeleted = true;
                            }

                            await StopLocalClock(info.Clock.ClockId, true);
                            await DeleteLocalClock(info.Clock.ClockId);
                            await _session.SaveToStorage(CurrentSession, SessionKey, _log);
                            RaiseUpdateEvent();
                        }
                        break;

                    case UpdateAction.UpdateClock:
                        if (info.Clock != null)
                        {
                            var existingClock = CurrentSession.Clocks
                                .FirstOrDefault(c => c.Message.ClockId == info.Clock.ClockId);

                            if (existingClock != null)
                            {
                                await StopLocalClock(info.Clock.ClockId, true);
                                existingClock.Update(info.Clock, true);
                                existingClock.CurrentLabel = info.Clock.Label;
                                await _session.SaveToStorage(CurrentSession, SessionKey, _log);
                                RaiseUpdateEvent();
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Unable to deserialize: {ex.Message}");
            }
        }

        internal async Task ResetState()
        {
            _session.State = 0;
            await UnregisterFromPreviousGroup(CurrentSession?.SessionId);
        }

        public async Task AddClockAfter(
            Clock previousClock,
            StartClockMessage newClockMessage = null)
        {
            _log.LogInformation("-> SignalRHost.AddClockAfter");

            if (previousClock != null)
            {
                var index = CurrentSession.Clocks.IndexOf(previousClock);

                if (index > -1)
                {
                    var newClock = new Clock();

                    if (newClockMessage != null)
                    {
                        newClock.Update(newClockMessage, true);
                        newClock.CurrentLabel = newClockMessage.Label;
                    }
                    else
                    {
                        newClock.Message.Position = index + 1;
                    }

                    newClock.SelectionChanged += ClockSelectionChanged;
                    CurrentSession.Clocks.Insert(index + 1, newClock);

                    if (newClockMessage == null)
                    {
                        _log.LogTrace("Updating other hosts");
                        await UpdateRemoteHosts(
                            UpdateAction.AddClock,
                            null,
                            newClock.Message,
                            previousClock.Message.ClockId);

                        await _session.Save(CurrentSession, SessionKey, _log);
                    }
                }

                for (var clockIndex = 0; clockIndex > CurrentSession.Clocks.Count; clockIndex++)
                {
                    CurrentSession.Clocks[clockIndex].Message.Position = clockIndex;
                }
            }
        }

        public void CancelDeleteSession()
        {
            _log.LogInformation("-> CancelDeleteSession");
            IsModifySessionDisabled = false;
            IsDeleteSessionWarningVisible = false;
        }

        public async Task CheckState()
        {
            _log.LogInformation("-> CheckState");
            var state = _session.State;
            _log.LogDebug($"State: {state}");

            await InitializePeerInfo();

            if (state == 0)
            {
                // Nothing yet --> Unregister and delete
                var session = await _session.GetFromStorage(SessionKey, _log);

                if (session != null)
                {
                    await UnregisterFromPreviousGroup(session.SessionId);
                }

                //await _session.DeleteFromStorage(SessionKey, _log);
                _session.State = 1;
                _log.LogTrace("Deleted session and set state to 1");
                return;
            }
        }

        public async Task ClearInputMessage()
        {
            InputMessage = "";
            await SendMessage(" ");
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRHost.Connect");

            IsBusy = true;
            IsInError = false;
            IsConnected = false;

            IsSendMessageDisabled = true;
            IsModifySessionDisabled = true;

            RaiseUpdateEvent();

            var ok = await InitializeSession(_sessionId);

            if (!ok)
            {
                if (!IsInError)
                {
                    _log.LogWarning("Interrupt after initializing session");
                    IsBusy = false;
                    IsConnected = false;
                    IsInError = false;
                    RaiseUpdateEvent();
                }

                return;
            }

            _log.LogTrace("Initializing guest info");

            if (_connection == null)
            {
                ok = await CreateConnection();

                if (ok)
                {
                    _connection.On<string>(Constants.PeerToHostMessageName, ReceiveGuestMessage);
                    _connection.On(Constants.HostToPeerRequestAnnounceMessageName, AnnounceName);
                    _connection.On<string>(Constants.DisconnectMessage, ReceiveDisconnectMessage);
                    _connection.On<string>(Constants.HostToPeerMessageName, DisplayReceivedMessage);
                    _connection.On<string>(Constants.StartClockMessageName, s => ReceiveStartClock(s, true));
                    _connection.On<string>(Constants.StopClockMessage, s => StopLocalClock(s, true));
                    _connection.On<string>(Constants.UpdateHostMessageName, UpdateLocalHost);

                    ok = await StartConnection();
                }
            }

            if (ok)
            {
                _log.LogTrace("CreateConnection and StartConnection OK");

                foreach (var clock in CurrentSession.Clocks)
                {
                    clock.IsPlayStopDisabled = false;
                    clock.IsConfigDisabled = false;
                    clock.IsNudgeDisabled = true;
                    clock.SelectionChanged += ClockSelectionChanged;

                    if (clock.Message.ServerTime + clock.Message.CountDown + clock.Message.Nudge > DateTime.Now)
                    {
                        clock.IsClockRunning = true;
                        _log.LogDebug($"Label: {clock.Message.Label}");
                        _log.LogDebug($"ServerTime: {clock.Message.ServerTime}");
                        _log.LogDebug($"CountDown: {clock.Message.CountDown}");
                        _log.LogDebug($"Nudge: {clock.Message.Nudge}");
                        _log.LogDebug($"{clock.Message.Label} still active");
                    }
                }

                await StartClocks(CurrentSession.Clocks.Where(c => c.IsClockRunning).ToList(), false);

                _log.LogDebug($"CurrentSession.LastMessage: {CurrentSession.LastMessage}");

                if (string.IsNullOrEmpty(CurrentSession.LastMessage))
                {
                    DisplayMessage("Ready", false);
                }
                else
                {
                    DisplayMessage(CurrentSession.LastMessage, false);
                }

                // Request all guests to announce themselves so we can have a correct count
                var result = await RequestAnnounce();

                if (!result)
                {
                    _log.LogWarning("Couldn't get an existing guest count");
                    // Continue anyway, this is a minor issue
                }

                ok = await AnnounceName();

                if (!ok)
                {
                    IsConnected = false;
                    IsInError = true;
                    IsBusy = false;
                    DisplayMessage("Error", true);
                    return;
                }

                IsSendMessageDisabled = false;
                IsModifySessionDisabled = false;
                Status = "Connected, your guests will only see clocks when you start them!";
                IsConnected = true;
                IsInError = false;
            }
            else
            {
                _log.LogTrace("StartConnection NOT OK");

                foreach (var clock in CurrentSession.Clocks)
                {
                    clock.IsPlayStopDisabled = true;
                    clock.IsConfigDisabled = true;
                    clock.IsNudgeDisabled = true;
                }

                IsSendMessageDisabled = true;
                IsModifySessionDisabled = false;
                Status = "Cannot connect";
                IsConnected = false;
                IsInError = true;
            }

            IsBusy = false;
            RaiseUpdateEvent();
            _log.LogInformation("SignalRHost.Connect ->");
        }

        public async Task DeleteClock(Clock clock)
        {
            clock.SelectionChanged -= ClockSelectionChanged;
            await DeleteLocalClock(clock.Message.ClockId);

            var success = await SaveSession();

            if (!success)
            {
                return;
            }

            await UpdateRemoteHosts(
                UpdateAction.DeleteClock,
                null,
                clock.Message,
                null);

            var isOneClockRunning = CurrentSession.Clocks.Any(c => c.IsClockRunning);
            IsModifySessionDisabled = isOneClockRunning;
            _log.LogInformation("DeleteClock ->");

        }

        public async Task DeleteSessionFromStorage()
        {
            await _session.DeleteFromStorage(SessionKey, _log);
        }

        public void DeleteSession()
        {
            _log.LogInformation("-> DeleteSession");
            IsModifySessionDisabled = true;
            IsDeleteSessionWarningVisible = true;
        }

        public async Task DoDeleteSession()
        {
            _log.LogInformation("-> DoDeleteSession");

            IsDeleteSessionWarningVisible = false;

            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
                _log.LogTrace("Connection is stopped and disposed");
            }

            await _session.DeleteFromStorage(SessionKey, _log);
            CurrentSession = null;
            _log.LogTrace("CurrentSession is deleted");

            IsModifySessionDisabled = true;
            Status = "Disconnected";

            _log.LogInformation("DoDeleteSession ->");
        }

        public async Task<bool> InitializeSession(string sessionId)
        {
            _log.LogInformation("-> SignalRHost.InitializeSession");

            _log.LogDebug($"SessionKey: {SessionKey}");
            _log.LogDebug($"sessionId: {sessionId}");

            if (string.IsNullOrEmpty(sessionId))
            {
                IsBusy = false;
                IsInError = false;
                IsConnected = false;
                IsSessionUnknown = false;
                _nav.NavigateTo("/session");
                return false;
            }

            CurrentSession = await _session.GetFromStorage(SessionKey, _log);

            _log.LogDebug($"CurrentSession is null: {CurrentSession == null}");

            if (CurrentSession == null
                || (!string.IsNullOrEmpty(sessionId)
                    && CurrentSession.SessionId != sessionId))
            {
                try
                {
                    var allSessions = await _session.GetSessions(_log);
                    CurrentSession = allSessions.FirstOrDefault(s => s.SessionId == sessionId);

                    if (CurrentSession == null)
                    {
                        _log.LogWarning($"Cannot find a session for {sessionId}");
                        IsBusy = false;
                        IsInError = true;
                        IsConnected = false;
                        IsSessionUnknown = true;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError($"Cannot get sessions: {ex.Message}");
                    IsConnected = false;
                    IsInError = true;
                    IsSessionUnknown = false;
                    ErrorStatus = "Error getting sessions";
                    RaiseUpdateEvent();
                    return false;
                }
            }
            else
            {
                // Refresh session

                try
                {
                    _log.LogDebug("Refreshing session");
                    var sessions = await _session.GetSessions(_log);
                    var outSession = sessions.FirstOrDefault(s => s.SessionId == CurrentSession.SessionId);

                    if (outSession == null)
                    {
                        _log.LogDebug("outSession is null, navigating");
                        _nav.NavigateTo("/session");
                        return false;
                    }

                    CurrentSession = outSession;
                }
                catch (Exception ex)
                {
                    _log.LogError($"Cannot get sessions: {ex.Message}");
                    IsConnected = false;
                    IsInError = true;
                    IsSessionUnknown = false;
                    ErrorStatus = "Error getting sessions";
                    RaiseUpdateEvent();
                    return false;
                }
            }

            foreach (var clock in CurrentSession.Clocks)
            {
                _log.LogDebug($"Setting clock {clock.Message.Label}");

                clock.IsPlayStopDisabled = true;
                clock.IsConfigDisabled = true;
                clock.IsClockRunning = false;
                clock.ClockDisplay = (clock.Message.CountDown + clock.Message.Nudge).ToString("c");
            }

            RaiseUpdateEvent();

            _log.LogInformation("SignalRHost.InitializeSession ->");
            return true;
        }

        public async Task Nudge(Clock clock, int seconds)
        {
            _log.LogInformation("-> Nudge");

            var clockInSession = CurrentSession.Clocks
                .FirstOrDefault(c => c.Message.ClockId == clock.Message.ClockId);

            if (clockInSession == null)
            {
                _log.LogWarning($"Clock not found: {clock.Message.ClockId}");
                return;
            }

            var timespan = TimeSpan.FromSeconds(Math.Abs(seconds));

            if (seconds > 0)
            {
                _log.LogDebug($"Adding {seconds} seconds");
                clockInSession.Message.Nudge += timespan;
            }
            else
            {
                _log.LogDebug($"Substracting {seconds} seconds");
                clockInSession.Message.Nudge -= timespan;
            }

            await _session.Save(CurrentSession, SessionKey, _log);
            await StartClock(clock, false); ;
        }

        public bool PrepareClockToConfigure(string clockId)
        {
            _log.LogInformation("-> PrepareClockToConfigure");

            var clock = CurrentSession.Clocks
                .FirstOrDefault(c => c.Message.ClockId == clockId);

            if (clock == null)
            {
                return false;
            }

            var param = new ConfigureClock
            {
                Host = this,
                CurrentClock = clock
            };

            Program.ClockToConfigure = param;
            _log.LogInformation("PrepareClockToConfigure ->");
            return true;
        }

        public void ReceiveDisconnectMessage(string guestId)
        {
            _log.LogInformation($"-> SignalRHost.{nameof(ReceiveDisconnectMessage)}");
            _log.LogDebug($"{nameof(guestId)} {guestId}");
            _log.LogDebug($"Local UserId: {PeerInfo.Message.PeerId}");

            var success = Guid.TryParse(guestId, out Guid guestGuid);

            if (!success
                || guestGuid == Guid.Empty)
            {
                _log.LogWarning($"GuestId is not a GUID");
                return;
            }

            var existingGuest = ConnectedPeers.FirstOrDefault(g => g.PeerId == guestId);

            if (existingGuest == null)
            {
                _log.LogWarning("No existing guest found");
                return;
            }

            ConnectedPeers.Remove(existingGuest);
            UpdateConnectedPeers(null);
            RaiseUpdateEvent();
            _log.LogInformation($"SignalRHost.{nameof(ReceiveDisconnectMessage)} ->");
        }

        public async Task ReceiveGuestMessage(string json)
        {
            _log.LogInformation($"-> SignalRHost.{nameof(ReceiveGuestMessage)}");
            //_log.LogDebug(json);

            var messagePeer = JsonConvert.DeserializeObject<PeerMessage>(json);

            if (messagePeer == null
                || string.IsNullOrEmpty(messagePeer.PeerId))
            {
                _log.LogWarning($"No PeerId found");
                return;
            }

            _log.LogDebug($"PeerId: {messagePeer.PeerId}");

            var success = Guid.TryParse(messagePeer.PeerId, out Guid peerId);

            if (!success
                || peerId == Guid.Empty)
            {
                _log.LogWarning($"PeerId is not a GUID");
                return;
            }

            if (messagePeer.PeerId == PeerInfo.Message.PeerId)
            {
                _log.LogTrace($"Self announce received");
                return;
            }

            var existingPeer = ConnectedPeers.FirstOrDefault(g => g.PeerId == messagePeer.PeerId);

            if (existingPeer == null)
            {
                _log.LogWarning("No existing guest found");
                UpdateConnectedPeers(messagePeer);
            }
            else
            {
                _log.LogDebug($"Existing peer found: Old name {existingPeer.DisplayName}");
                existingPeer.CustomName = messagePeer.CustomName;
                existingPeer.IsHost = messagePeer.IsHost;

                UpdateConnectedPeers(null);
                _log.LogDebug($"Existing guest found: New name {existingPeer.DisplayName}");
            }

            // Refresh the clocks and the message (only when a guest registers)

            _log.LogDebug($"messagePeer.IsHost {messagePeer.IsHost}");

            if (!messagePeer.IsHost)
            {
                if (IsAnyClockRunning)
                {
                    await StartAllClocks(false);
                }

                await SendMessage(CurrentMessage.Value);
            }

            RaiseUpdateEvent();
            _log.LogInformation($"SignalRHost.{nameof(ReceiveGuestMessage)} ->");
        }

        public async Task SendInputMessage()
        {
            await SendMessage(InputMessage.Trim());
        }

        public async Task SendMessage(string message)
        {
            _log.LogInformation($"-> {nameof(SendMessage)}");
            _log.LogDebug($"Sender ID: {PeerInfo.Message.PeerId}");

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            try
            {
                var htmlMessage = message
                    .Trim()
                    .Replace("\n", "<br />");

                var opening = true;
                int index = -1;

                do
                {
                    index = htmlMessage.IndexOf("*");

                    if (index > -1)
                    {
                        htmlMessage = htmlMessage[..index]
                            + (opening ? "<span style='color: red'>" : "</span>")
                            + htmlMessage[(index + 1)..];

                        opening = !opening;
                    }
                } while (index >= 0);

                if (index > 0
                    && !opening)
                {
                    htmlMessage += "</span>";
                }

                DisplayMessage(htmlMessage, false);

                var content = new StringContent(htmlMessage);

                var sendMessageUrl = $"{_hostName}/send";
                _log.LogDebug($"sendMessageUrl: {sendMessageUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, sendMessageUrl);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogError($"Cannot send message: {response.ReasonPhrase}");
                    ErrorStatus = "Error sending message";
                }
                else
                {
                    Status = "Message sent";
                    CurrentSession.LastMessage = htmlMessage;
                    await _session.Save(CurrentSession, SessionKey, _log);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Cannot send message: {ex.Message}");
                ErrorStatus = "Error sending message";
            }

            _log.LogInformation($"{nameof(SendMessage)} ->");
        }

        public async Task StartAllClocks(bool startFresh)
        {
            await StartClocks(CurrentSession.Clocks.ToList(), startFresh);
        }

        public async Task StartClock(Clock clock, bool startFresh)
        {
            _log.LogInformation($"SignalRHost.StartClock {clock.Message.Label}");

            await StartClocks(new List<Clock>
                {
                    clock
                },
                startFresh);
        }

        public async Task StartClocks(
            IList<Clock> clocks,
            bool startFresh)
        {
            _log.LogInformation("-> StartClocks");
            _log.LogDebug($"Sender ID: {PeerInfo.Message.PeerId}");
            StartClocksButtonText = StartAllClocksText;

            var clocksToStart = clocks.ToList();

            if (clocks.Any(c => c.IsSelected))
            {
                clocksToStart = clocks.Where(c => c.IsSelected).ToList();
            }

            _log.LogDebug($"{clocksToStart.Count} clock(s) to start");

            if (startFresh)
            {
                var activeClocks = clocksToStart
                    .Where(c => c.IsClockRunning)
                    .ToList();

                foreach (var activeClock in activeClocks)
                {
                    activeClock.IsSelected = false;
                    clocksToStart.Remove(activeClock);
                }

                if (clocksToStart.Count == 0)
                {
                    return;
                }
            }
            else
            {
                var inactiveClocks = clocksToStart
                    .Where(c => !c.IsClockRunning)
                    .ToList();

                foreach (var inactiveClock in inactiveClocks)
                {
                    inactiveClock.IsSelected = false;
                    clocksToStart.Remove(inactiveClock);
                }

                if (clocksToStart.Count == 0)
                {
                    _log.LogTrace("No more active clocks");
                    return;
                }
            }

            _log.LogInformation($"-> SignalRHost.StartClocks {clocksToStart.Count} clock(s)");

            IsModifySessionDisabled = true;

            try
            {
                if (startFresh)
                {
                    foreach (var clock in clocksToStart)
                    {
                        _log.LogDebug($"Reset clock {clock.Message.Label}");
                        clock.Reset();
                    }

                    // Save so that we can restart the clock if the page is reloaded
                    await _session.Save(CurrentSession, SessionKey, _log);
                }

                var json = JsonConvert.SerializeObject(CurrentSession.Clocks
                    .OrderBy(c => c.Message.Position)
                    .Select(c =>
                    {
                        c.Message.SenderId = PeerInfo.Message.PeerId;
                        c.Message.SessionName = CurrentSession.SessionName;
                        return c.Message;
                    })
                    .ToList());

                var content = new StringContent(json);

                //_log.LogDebug($"json: {json}");

                var startClockUrl = $"{_hostName}/start";
                _log.LogDebug($"startClockUrl: {startClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, startClockUrl);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    foreach (var clock in clocksToStart)
                    {
                        RunClock(clock);
                    }
                }
                else
                {
                    ErrorStatus = "Unable to communicate with clients";
                    IsModifySessionDisabled = false;
                }
            }
            catch
            {
                DisplayMessage("Unable to communicate with clients", true);
                IsModifySessionDisabled = false;
            }
        }

        public async Task StopClock(Clock clock)
        {
            _log.LogInformation("-> StopClock");

            await StopLocalClock(clock.Message.ClockId, true);

            // Notify clients

            try
            {
                var stopClockUrl = $"{_hostName}/stop";
                _log.LogDebug($"stopClockUrl: {stopClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, stopClockUrl);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

                var content = new StringContent(clock.Message.ClockId);
                httpRequest.Content = content;
                var response = await _http.SendAsync(httpRequest);

                _log.LogDebug($"Response code: {response.StatusCode}");
                _log.LogDebug($"Response phrase: {response.ReasonPhrase}");

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogError($"Error sending stop instruction: {response.ReasonPhrase}");
                    ErrorStatus = "Couldn't reach the guests";
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error sending stop instruction: {ex.Message}");
                ErrorStatus = "Couldn't reach the guests";
            }

            await _session.Save(CurrentSession, SessionKey, _log);

            clock.IsConfigDisabled = false;
            clock.IsNudgeDisabled = true;
            clock.ResetDisplay();

            IsModifySessionDisabled = IsAnyClockRunning;

            _log.LogInformation("StopClock ->");
        }

        public async Task UpdateRemoteHosts(
                                                                                                                                    UpdateAction action,
            string sessionName,
            StartClockMessage clock,
            string previousClockId)
        {
            _log.LogInformation("-> SignalRHost.UpdateRemoteHosts");

            var info = new UpdateHostInfo
            {
                SessionName = sessionName,
                Clock = clock,
                PreviousClockId = previousClockId,
                Action = action
            };

            var json = JsonConvert.SerializeObject(info);

            var content = new StringContent(json);

            var updateUrl = $"{_hostName}/update";
            _log.LogDebug($"updateUrl: {updateUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, updateUrl);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
            httpRequest.Content = content;

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Cannot send update: {response.ReasonPhrase}");
                ErrorStatus = "Error sending update";
            }
        }
    }
}