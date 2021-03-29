using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public abstract class SignalRHandler
    {
        public event EventHandler UpdateUi;

        private readonly ILocalStorageService _localStorage;
        private string _errorStatus;
        private string _status;
        protected const string AnnounceGuestKeyKey = "AnnounceGuestKey";
        protected const string FunctionCodeHeaderKey = "x-functions-key";
        protected const string HostNameFreeKey = "HostNameFree";
        protected const string HostNameKey = "HostName";
        protected const string NegotiateKeyKey = "NegotiateKey";
        protected const string RegisterKeyKey = "RegisterKey";
        protected const string SendMessageKeyKey = "SendMessageKey";
        protected const string StartClockKeyKey = "StartClockKey";
        protected const string StopClockKeyKey = "StopClockKey";
        protected const string UnregisterKeyKey = "UnregisterKey";
        protected IConfiguration _config;
        protected HubConnection _connection;

        protected string _hostName;

        protected string _hostNameFree;

        protected HttpClient _http;

        protected ILogger _log;

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

        public bool IsTaskRunning
        {
            get;
            private set;
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

        public bool IsCreateNewSessionDisabled
        {
            get;
            protected set;
        }

        public bool IsDeleteSessionDisabled
        {
            get;
            protected set;
        }

        public bool IsInError
        {
            get;
            protected set;
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
            HttpClient http)
        {
            DisplayMessage("Welcome!", false);
            Status = "Please wait...";

            _config = config;
            _localStorage = localStorage;
            _log = log;
            _http = http;
            SessionBase.SetLocalStorage(_localStorage);

            _hostName = _config.GetValue<string>(HostNameKey);
            _hostNameFree = _config.GetValue<string>(HostNameFreeKey);
            _log.LogDebug($"_hostName: {_hostName}");
            _log.LogDebug($"_hostNameFree: {_hostNameFree}");
        }

        private Task ConnectionReconnected(string arg)
        {
            var tcs = new TaskCompletionSource<bool>();
            Status = "Reconnected!";
            tcs.SetResult(true);
            return tcs.Task;
        }

        private Task ConnectionReconnecting(Exception arg)
        {
            var tcs = new TaskCompletionSource<bool>();

            ErrorStatus = "Lost connection, trying to reconnect...";
            IsBusy = true;
            IsConnected = false;
            IsInError = true;

            // TODO Should also disable the controls for the Host

            tcs.SetResult(true);
            return tcs.Task;
        }

        private async Task<bool> RegisterToGroup()
        {
            try
            {
                var registerUrl = $"{_hostNameFree}/register";
                _log.LogDebug($"registerUrl: {registerUrl}");

                var functionKey = _config.GetValue<string>(RegisterKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                _log.LogDebug($"SessionId: {CurrentSession.SessionId}");
                _log.LogDebug($"UserId: {CurrentSession.UserId}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, registerUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

                var registerInfo = new UserInfo
                {
                    UserId = CurrentSession.UserId
                };

                var content = new StringContent(JsonConvert.SerializeObject(registerInfo));
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _log.LogError($"Error registering for group: {response.ReasonPhrase}");
                    ErrorStatus = "Error with the backend, please contact support";
                    IsInError = true;
                    _log.LogInformation("SignalRHandler.CreateConnection ->");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the function: {ex.Message}");
                ErrorStatus = "Error with the backend, please contact support";
                IsInError = true;
                _log.LogInformation("SignalRHandler.CreateConnection ->");
                return false;
            }

            return true;
        }

        protected async Task<bool> CreateConnection()
        {
            _log.LogInformation("-> SignalRHandler.CreateConnection");

            NegotiateInfo negotiateInfo = null;

            try
            {
                var functionKey = _config.GetValue<string>(NegotiateKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var negotiateUrl = $"{_hostNameFree}/negotiate";
                _log.LogDebug($"negotiateUrl: {negotiateUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Get, negotiateUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.UserIdHeaderKey, CurrentSession.UserId);

                _log.LogDebug($"UserId: {CurrentSession.UserId}");

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
                    IsInError = true;
                    _log.LogInformation("SignalRHandler.CreateConnection ->");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the function: {ex.Message}");
                ErrorStatus = "Error with the backend, please contact support";
                IsInError = true;
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
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the SignalR service: {ex.Message}");
                ErrorStatus = "Error connecting, please contact support";
                return false;
            }

            var ok = await RegisterToGroup(); // TODO handle failure

            if (!ok)
            {
                return false;
            }

            Status = "Ready...";
            _log.LogInformation("SignalRHandler.CreateConnection ->");
            return true;
        }

        protected virtual async Task DeleteLocalClock(string clockId)
        {
            _log.LogInformation("-> DeleteLocalClock");

            var clock = CurrentSession.Clocks.FirstOrDefault(c => c.Message.ClockId == clockId);

            if (clock == null
                || clock.IsClockRunning)
            {
                _log.LogTrace($"No clock found with id {clockId}");
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
            _log?.LogInformation("-> DisplayMessage");
            _log?.LogDebug(message);

            if (wrapInError)
            {
                message = $"<span syle='color: red'>{message}</span>";
            }

            CurrentMessage = new MarkupString(message);
            RaiseUpdateEvent();
        }

        protected void RaiseUpdateEvent()
        {
            UpdateUi?.Invoke(this, EventArgs.Empty);
        }

        protected abstract string SessionKey
        {
            get;
        }

        protected async Task RestoreClock(Clock clock)
        {
            // Get saved clock and restore
            var savedSession = await SessionBase.GetFromStorage(SessionKey, _log);
            var clockInSavedSession = savedSession.Clocks
                .FirstOrDefault(c => c.Message.ClockId == clock.Message.ClockId);

            if (clockInSavedSession != null)
            {
                clock.Restore(clockInSavedSession);
            }
        }

        protected void RunClock(Clock activeClock)
        {
            _log.LogInformation($"-> {nameof(RunClock)}");
            _log.LogDebug($"ClockId: {activeClock.Message.ClockId}");
            _log.LogDebug($"CurrentBackgroundColor: {activeClock.CurrentBackgroundColor}");
            _log.LogDebug($"Label: {activeClock.Message.Label}");

            if (IsTaskRunning)
            {
                _log.LogTrace("Clock task already running");
                activeClock.IsClockRunning = true;
                return;
            }

            activeClock.IsClockRunning = true;

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

                            if (remains.TotalSeconds <= 0)
                            {
                                _log.LogTrace("Countdown finished");
                                clock.IsClockRunning = false;
                                clock.IsNudgeDisabled = true;
                                clock.ClockDisplay = Clock.DefaultClockDisplay;
                                clock.CurrentBackgroundColor = clock.Message.AlmostDoneColor;
                                clock.Message.ServerTime = DateTime.MinValue;
                                Status = $"Countdown finished for {clock.Message.Label}";
                                clock.RaiseCountdownFinished();
                                RaiseUpdateEvent();
                                await RestoreClock(clock);
                                await CurrentSession.Save(SessionKey, _log);
                                continue;
                            }

                            clock.CurrentBackgroundColor = clock.Message.RunningColor;

                            if (Math.Floor(remains.TotalSeconds) <= clock.Message.PayAttention.TotalSeconds)
                            {
                                clock.CurrentBackgroundColor = clock.Message.PayAttentionColor;
                            }

                            if (Math.Floor(remains.TotalSeconds) <= clock.Message.AlmostDone.TotalSeconds)
                            {
                                clock.CurrentBackgroundColor = clock.Message.AlmostDoneColor;
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

            if (keepClock)
            {
                existingClock.IsClockRunning = false;
                existingClock.Message.ServerTime = DateTime.MinValue;
                existingClock.Message.CountDown = existingClock.Message.ConfiguredCountDown;
                existingClock.Message.ConfiguredCountDown = TimeSpan.FromSeconds(0);
                _log.LogDebug($"HIGHLIGHT--existingClock.Message.ConfiguredCountDown {existingClock.Message.ConfiguredCountDown}");
                existingClock.ResetDisplay();
            }
            else
            {
                CurrentSession.Clocks.Remove(existingClock);
            }

            await CurrentSession.Save(SessionKey, _log);

            Status = $"Clock {existingClock.Message.Label} was stopped";
            RaiseUpdateEvent();
            _log.LogInformation($"{nameof(StopLocalClock)} ->");
        }

        public abstract Task Connect(
            string templateName = null);

        public async Task Disconnect()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
                _log.LogTrace("Connection is stopped and disposed");
            }
        }

        public async Task SaveSession()
        {
            await CurrentSession.Save(SessionKey, _log);
        }
    }
}