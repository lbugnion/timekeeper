using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public abstract class SignalRHandler
    {
        public event EventHandler UpdateUi;

        private string _errorStatus;
        private string _status;
        protected const string AnnounceGuestKeyKey = "AnnounceGuestKey";
        protected const string FunctionCodeHeaderKey = "x-functions-key";
        protected const string NegotiateKeyKey = "NegotiateKey";
        protected const string RegisterKeyKey = "RegisterKey";
        protected const string SendMessageKeyKey = "SendMessageKey";
        protected const string StartClockKeyKey = "StartClockKey";
        protected const string StopClockKeyKey = "StopClockKey";
        protected const string UnregisterKeyKey = "UnregisterKey";
        protected readonly IConfiguration _config;
        protected readonly HttpClient _http;
        protected readonly ILogger _log;
        protected readonly SessionHandler _session;
        protected HubConnection _connection;

        protected string _hostName;
        protected string _hostNameFree;
        private bool _isManualDisconnection;

        protected abstract string SessionKey
        {
            get;
        }

        protected abstract string PeerKey
        {
            get;
        }

        public MarkupString CurrentMessage
        {
            get;
            protected set;
        }

        public SessionBase CurrentSession
        {
            get;
            protected set;
        }

        public string ErrorStatus
        {
            get => _errorStatus;
            protected set
            {
                _errorStatus = value;

                if (!string.IsNullOrEmpty(_errorStatus))
                {
                    Status = string.Empty;
                }
            }
        }

        public bool IsAnyClockRunning
        {
            get
            {
                return CurrentSession.Clocks.Any(c => c.IsClockRunning);
            }
        }

        public bool IsBusy
        {
            get;
            protected set;
        }

        public bool IsConnected
        {
            get;
            protected set;
        }

        public bool IsInError
        {
            get;
            protected set;
        }

        public bool IsTaskRunning
        {
            get;
            private set;
        }

        public Peer PeerInfo
        {
            get;
            private set;
        }

        public string Status
        {
            get => _status;
            protected set
            {
                _status = value;

                if (!string.IsNullOrEmpty(_status))
                {
                    ErrorStatus = string.Empty;
                }
            }
        }

        public SignalRHandler(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            SessionHandler session)
        {
            DisplayMessage("Welcome!", false);
            Status = "Please wait...";

            _config = config;
            _log = log;
            _http = http;
            _session = session;

            Peer.SetLocalStorage(localStorage, log);

            _hostName = _config.GetValue<string>(Constants.HostNameKey);
            _hostNameFree = _config.GetValue<string>(Constants.HostNameFreeKey);
            _log.LogDebug($"_hostName: {_hostName}");
            _log.LogDebug($"_hostNameFree: {_hostNameFree}");
        }

        private Task ConnectionReconnected(string arg)
        {
            var tcs = new TaskCompletionSource<bool>();
            Status = "Reconnected!";

            IsBusy = false;
            IsConnected = true;
            IsInError = false;

            RaiseUpdateEvent();

            tcs.SetResult(true);
            return tcs.Task;
        }

        private Task ConnectionReconnecting(Exception arg)
        {
            var tcs = new TaskCompletionSource<bool>();

            ErrorStatus = "Lost connection, trying to reconnect...";
            IsBusy = true;
            IsConnected = false;
            IsInError = false;

            RaiseUpdateEvent();

            tcs.SetResult(true);
            return tcs.Task;
        }

        private async Task<bool> RegisterToGroup()
        {
            _log.LogInformation("-> RegisterToGroup");

            try
            {
                var registerUrl = $"{_hostNameFree}/register";
                _log.LogDebug($"registerUrl: {registerUrl}");

                var functionKey = _config.GetValue<string>(RegisterKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, registerUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

                var registerInfo = new UserInfo
                {
                    UserId = PeerInfo.Message.PeerId
                };

                var content = new StringContent(JsonConvert.SerializeObject(registerInfo));
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogError($"Error registering for group: {response.ReasonPhrase}");
                    ErrorStatus = "Error with the backend, please contact support";
                    _log.LogInformation("SignalRHandler.RegisterToGroup ->");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the function: {ex.Message}");
                ErrorStatus = "Error with the backend, please contact support";
                _log.LogInformation("SignalRHandler.RegisterToGroup ->");
                return false;
            }

            _log.LogInformation("SignalRHandler.RegisterToGroup ->");
            return true;
        }

        protected async Task<bool> AnnounceNameJson(string json)
        {
            var content = new StringContent(json);

            var functionKey = _config.GetValue<string>(AnnounceGuestKeyKey);
            _log.LogDebug($"functionKey: {functionKey}");

            var announceUrl = $"{_hostNameFree}/announce";
            _log.LogDebug($"announceUrl: {announceUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, announceUrl);
            httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
            httpRequest.Content = content;

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Cannot send message: {response.ReasonPhrase}");
                _log.LogInformation($"{nameof(AnnounceNameJson)} ->");
                return false;
            }

            _log.LogInformation($"{nameof(AnnounceNameJson)} ->");
            return true;
        }

        protected async Task<bool> CreateConnection()
        {
            _log.LogInformation("-> SignalRHandler.CreateConnection");

            NegotiateInfo negotiateInfo = null;

            try
            {
#if !OFFLINE
                var functionKey = _config.GetValue<string>(NegotiateKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var negotiateUrl = $"{_hostNameFree}/negotiate";
                _log.LogDebug($"negotiateUrl: {negotiateUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Get, negotiateUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.UserIdHeaderKey, PeerInfo.Message.PeerId);

                _log.LogDebug($"UserId: {PeerInfo.Message.PeerId}");

                var response = await _http.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var negotiateJson = await response.Content.ReadAsStringAsync();
                    negotiateInfo = JsonConvert.DeserializeObject<NegotiateInfo>(negotiateJson);
                }
                else
                {
                    _log.LogError($"Error reaching the function: {response.ReasonPhrase}");
                    ErrorStatus = "Error with the backend, please contact support";
                    _log.LogInformation("SignalRHandler.CreateConnection ->");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the function: {ex.Message}");
                ErrorStatus = "Error with the backend, please contact support";
                _log.LogInformation("SignalRHandler.CreateConnection ->");
                return false;
            }

            _log.LogDebug($"HubName: {negotiateInfo.HubName}");

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(negotiateInfo.Url, options =>
                    {
                        options.AccessTokenProvider = async () => negotiateInfo.AccessToken;
                    })
                    .WithAutomaticReconnect()
                    .Build();

                _connection.Reconnecting += ConnectionReconnecting;
                _connection.Reconnected += ConnectionReconnected;
                _connection.Closed += ConnectionClosed;

#endif
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the SignalR service: {ex.Message}");
                ErrorStatus = "Error connecting, please contact support";
                return false;
            }

#if !OFFLINE
            var ok = await RegisterToGroup();

            if (!ok)
            {
                return false;
            }
#endif

            Status = "Ready...";
            _log.LogInformation("SignalRHandler.CreateConnection ->");
            return true;
        }

        private Task ConnectionClosed(Exception arg)
        {
            _log.LogWarning(nameof(ConnectionClosed));
            _log.LogDebug($"_isManualDisconnection: {_isManualDisconnection}");
            var tcs = new TaskCompletionSource<bool>();

            ErrorStatus = "Unable to reconnect, please refresh the page...";
            IsBusy = false;
            IsConnected = false;

            if (!_isManualDisconnection)
            {
                _log.LogTrace("HIGHLIGHT--Showing disconnected message");
                IsInError = true;
            }
            else
            {
                IsInError = false;
            }

            _isManualDisconnection = false;

            RaiseUpdateEvent();

            tcs.SetResult(true);
            return tcs.Task;
        }

        protected virtual async Task DeleteLocalClock(string clockId)
        {
            _log.LogInformation("-> DeleteLocalClock");

            var clock = CurrentSession.Clocks.FirstOrDefault(c => c.Message.ClockId == clockId);

            if (clock == null)
            {
                _log.LogTrace($"No clock found with id {clockId}");
                return;
            }

            if (clock.IsClockRunning)
            {
                _log.LogTrace($"Clock {clockId} is still running");
                return;
            }

            await StopLocalClock(clockId, true);

            _log.LogDebug("Clock stopped, removing");

            CurrentSession.Clocks.Remove(clock);
            RaiseUpdateEvent();

            _log.LogDebug("Removed");

            _log.LogDebug($"Remaining clocks: {CurrentSession.Clocks.Count}");
        }

        protected virtual void DisplayMessage(string message, bool wrapInError)
        {
            // _log might be null in this call!!

            _log?.LogInformation("-> DisplayMessage");
            _log?.LogDebug($"message: {message}");

            if (wrapInError)
            {
                message = $"<span syle='color: red'>{message}</span>";
            }

            CurrentMessage = new MarkupString(message);
            Status = "Received host message";
            RaiseUpdateEvent();
            _log?.LogInformation("DisplayMessage ->");
        }

        protected void DisplayReceivedMessage(string message)
        {
            DisplayMessage(message, false);
            Status = "Received host message";
        }

        protected async Task<bool> InitializePeerInfo()
        {
            _log.LogInformation("-> InitializePeerInfo");

            var message = await Peer.GetFromStorage(PeerKey);

            if (message == null)
            {
                _log.LogTrace("Saved PeerInfo is null");
                PeerInfo = new Peer(Guid.NewGuid().ToString());
                await PeerInfo.Save(PeerKey);
            }
            else
            {
                PeerInfo = new Peer(message.PeerId)
                {
                    Message = message
                };
            }

            _log.LogDebug($"guest ID: {PeerInfo.Message.PeerId}");
            _log.LogDebug($"name: {PeerInfo.Message.DisplayName}");
            _log.LogInformation("InitializeGuestInfo ->");
            return true;
        }

        protected void RaiseUpdateEvent()
        {
            UpdateUi?.Invoke(this, EventArgs.Empty);
        }

        protected void ReceiveStartClock(string message, bool keepClocks)
        {
            _log.LogInformation("-> SignalRGuest.ReceiveStartClock");
            _log.LogDebug(message);

            IList<StartClockMessage> clockMessages;

            try
            {
                clockMessages = JsonConvert.DeserializeObject<IList<StartClockMessage>>(message);

                if (clockMessages.Count == 0)
                {
                    return;
                }

                var senderId = clockMessages.First().SenderId;
                if (senderId == PeerInfo.Message.PeerId)
                {
                    _log.LogTrace("Self start clock received");
                    return;
                }
            }
            catch
            {
                _log.LogWarning("Not a list of clocks");
                return;
            }

            var clockStarted = 0;
            var newList = new List<Clock>();

            foreach (var clockMessage in clockMessages)
            {
                _log.LogDebug($"clockID: {clockMessage.ClockId}");
                _log.LogDebug($"Clock Label: {clockMessage.Label}");

                _log.LogDebug($"{clockMessage.ServerTime}");

                if (clockMessage.ServerTime.Year <= 1)
                {
                    continue;
                }

                var existingClock = CurrentSession.Clocks
                    .FirstOrDefault(c => c.Message.ClockId == clockMessage.ClockId);

                if (existingClock == null)
                {
                    _log.LogTrace($"No found clock, adding");
                    existingClock = new Clock(clockMessage);
                    clockStarted++;
                }
                else
                {
                    _log.LogDebug($"Found clock {existingClock.Message.Label}, updating");
                    existingClock.Update(clockMessage, false);
                    existingClock.CurrentLabel = clockMessage.Label;
                }

                _log.LogDebug($"Clock {existingClock.Message.Label} remains {existingClock.Remains}");
                newList.Add(existingClock);
            }

            if (newList.Count > 0)
            {
                Status = $"{newList.Count} clock(s) started";
            }

            if (!keepClocks)
            {
                CurrentSession.Clocks.Clear();

                foreach (var clock in newList.OrderBy(c => c.Message.Position))
                {
                    CurrentSession.Clocks.Add(clock);
                }
            }

            foreach (var clock in newList)
            {
                _log.LogDebug($"isClockRunning: {clock.Message.Label} : {clock.IsClockRunning}");
                RunClock(clock);
            }

            RaiseUpdateEvent();
            _log.LogInformation("SignalRGuest.ReceiveStartClock ->");
        }

        protected void RunClock(Clock activeClock)
        {
            _log.LogInformation($"-> {nameof(RunClock)}");
            _log.LogDebug($"ClockId: {activeClock.Message.ClockId}");
            _log.LogDebug($"CurrentBackgroundColor: {activeClock.CurrentBackgroundColor}");
            _log.LogDebug($"Label: {activeClock.Message.Label}");

            activeClock.IsSelected = false;
            activeClock.IsPlayStopDisabled = false;
            activeClock.IsConfigDisabled = true;
            activeClock.IsNudgeDisabled = false;
            activeClock.IsClockRunning = true;

            if (IsTaskRunning)
            {
                _log.LogTrace("Clock task already running");
                return;
            }

            Task.Run(async () =>
            {
                do
                {
                    IsTaskRunning = true;

                    if (CurrentSession.Clocks.Count == 0)
                    {
                        _log.LogTrace("No clocks found");
                        return;
                    }

                    foreach (var clock in CurrentSession.Clocks)
                    {
                        if (clock.IsClockRunning)
                        {
                            var remains = clock.Remains;

                            clock.CurrentBackgroundColor = clock.Message.RunningColor;
                            clock.CurrentForegroundColor = Clock.DefaultForegroundColor;
                            clock.CurrentLabel = clock.Message.Label;

                            if (Math.Floor(remains.TotalSeconds) <= clock.Message.PayAttention.TotalSeconds)
                            {
                                clock.CurrentBackgroundColor = clock.Message.PayAttentionColor;
                            }

                            if (Math.Floor(remains.TotalSeconds) <= clock.Message.AlmostDone.TotalSeconds)
                            {
                                clock.CurrentBackgroundColor = clock.Message.AlmostDoneColor;
                            }

                            if (remains.TotalSeconds <= 0)
                            {
                                if (Math.Floor(remains.TotalSeconds) % 2 == 0)
                                {
                                    clock.CurrentBackgroundColor = clock.Message.AlmostDoneColor;
                                    clock.CurrentLabel = clock.Message.Label;
                                }
                                else
                                {
                                    clock.CurrentBackgroundColor = clock.Message.PayAttentionColor;
                                    clock.CurrentForegroundColor = Clock.OvertimeForegroundColor;
                                    clock.CurrentLabel = clock.Message.OvertimeLabel;
                                }
                            }

                            clock.ClockDisplay = remains.ToString(@"hh\:mm\:ss");
                        }
                    }

                    RaiseUpdateEvent();

                    var delay = 1000 - DateTime.Now.Millisecond;
                    await Task.Delay(delay);
                }
                while (IsAnyClockRunning);

                IsTaskRunning = false;
            });
        }

        protected async Task<bool> StartConnection()
        {
            _log.LogInformation("-> SignalRHandler.StartConnection");

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                _log.LogError($"Error starting the connection: {ex.Message}");
                ErrorStatus = "Error connecting, please contact support";
                return false;
            }

            Status = "Connected!";

            _log.LogInformation("SignalRHandler.StartConnection ->");
            return true;
        }

        protected virtual async Task StopLocalClock(string clockId, bool keepClock)
        {
            _log.LogInformation($"-> {nameof(StopLocalClock)}");
            _log.LogDebug($"clockId: {clockId}");
            _log.LogDebug($"keepClock: {keepClock}");

            if (string.IsNullOrEmpty(clockId))
            {
                _log.LogWarning("Empty clockId");
                return;
            }

            var existingClock = CurrentSession.Clocks
                .FirstOrDefault(c => c.Message.ClockId == clockId);

            if (existingClock == null)
            {
                _log.LogTrace("No clock found");
                return;
            }

            if (!existingClock.IsClockRunning)
            {
                _log.LogTrace("Clock is not running");
                return;
            }

            if (keepClock)
            {
                existingClock.IsClockRunning = false;
                existingClock.IsPlayStopDisabled = false;
                existingClock.IsNudgeDisabled = true;
                existingClock.IsConfigDisabled = false;
                _log.LogDebug($"CountDown {existingClock.Message.CountDown}");
                _log.LogDebug($"ServerTime {existingClock.Message.ServerTime}");
                existingClock.Message.Nudge = TimeSpan.FromSeconds(0);
                existingClock.ResetDisplay();
                existingClock.Message.ServerTime = DateTime.MinValue;
                await _session.SaveToStorage(CurrentSession, SessionKey, _log);
            }
            else
            {
                _log.LogTrace("Removing clock in StopLocalClock");
                CurrentSession.Clocks.Remove(existingClock);
                await _session.SaveToStorage(CurrentSession, SessionKey, _log);
            }

            Status = $"Clock {existingClock.Message.Label} was stopped";
            RaiseUpdateEvent();
            _log.LogInformation($"{nameof(StopLocalClock)} ->");
        }

        protected async Task<bool> UnregisterFromPreviousGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return true;
            }

            _log.LogInformation("-> SignalRHandler.UnregisterFromPreviousGroup");

            try
            {
                var unregisterUrl = $"{_hostNameFree}/unregister";
                _log.LogDebug($"unregisterUrl: {unregisterUrl}");

                var functionKey = _config.GetValue<string>(UnregisterKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                _log.LogDebug($"Group ID: {groupId}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, unregisterUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, groupId);

                var registerInfo = new UserInfo
                {
                    UserId = PeerInfo.Message.PeerId
                };

                _log.LogDebug($"UserId: {PeerInfo.Message.PeerId}");

                var content = new StringContent(JsonConvert.SerializeObject(registerInfo));
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogError($"Error unregistering from group: {response.ReasonPhrase}");
                    _log.LogInformation("SignalRHandler.UnregisterFromPreviousGroup ->");
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the function: {ex.Message}");
                _log.LogInformation("SignalRHandler.UnregisterFromPreviousGroup ->");
            }

            return true;
        }

        public abstract Task Connect();

        public async Task Disconnect()
        {
            if (_connection != null)
            {
                _isManualDisconnection = true;
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
                _log.LogTrace("Connection is stopped and disposed");
            }
        }
    }
}