using Blazored.LocalStorage;
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
        public const string DefaultRunningColor = "#3AFFA9";
        public const string DefaultPayAttentionColor = "#FFFB91";
        public const string DefaultAlmostDoneColor = "#FF6B77";

        public event EventHandler CountdownFinished;
        public event EventHandler UpdateUi;

        private string _errorStatus;
        private string _status;
        protected const string FunctionCodeHeaderKey = "x-functions-key";
        protected const string HostNameKey = "HostName";
        protected const string HostNameFreeKey = "HostNameFree";
        protected const string NegotiateKeyKey = "NegotiateKey";
        protected const string SendMessageKeyKey = "SendMessageKey";
        protected const string AnnounceGuestKeyKey = "AnnounceGuestKey";
        protected const string StartClockKeyKey = "StartClockKey";
        protected const string StopClockKeyKey = "StopClockKey";
        protected const string RegisterKeyKey = "RegisterKey";
        protected const string UnregisterKeyKey = "UnregisterKey";
        protected IConfiguration _config;
        protected HubConnection _connection;

        public bool IsDeleteSessionDisabled 
        { 
            get; 
            protected set; 
        }

        public bool IsCreateNewSessionDisabled
        {
            get;
            protected set;
        }

        protected string _hostName;
        protected string _hostNameFree;
        protected HttpClient _http;
        protected ILogger _log;

        public string CurrentMessage
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
            CurrentMessage = "Welcome!";
            Status = "Please wait...";

            _config = config;
            _localStorage = localStorage;
            Session.SetLocalStorage(_localStorage);
            _log = log;
            _http = http;
        }

        public Session CurrentSession
        {
            get;
            protected set;
        }

        private readonly ILocalStorageService _localStorage;

        public async Task<bool> InitializeSession(
            string sessionId = null)
        {
            _log.LogInformation("-> InitializeSession");
            _log.LogDebug($"sessionId: {sessionId}");

            if (string.IsNullOrEmpty(sessionId))
            {
                CurrentSession = await Session.GetFromStorage();
            }

            if (CurrentSession == null)
            {
                _log.LogTrace("HIGHLIGHT--CurrentSession is null");

                CurrentSession = new Session
                {
                    SessionId = string.IsNullOrEmpty(sessionId) ? Guid.NewGuid().ToString() : sessionId,
                    SessionName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    UserId = Guid.NewGuid().ToString(),

                    ClockMessages = new List<StartClockMessage>
                    {
                        new StartClockMessage
                        {
                            CountDown = TimeSpan.FromMinutes(5),
                            AlmostDone = TimeSpan.FromSeconds(30),
                            PayAttention = TimeSpan.FromSeconds(120),
                            RunningColor = DefaultRunningColor,
                            PayAttentionColor = DefaultPayAttentionColor,
                            AlmostDoneColor = DefaultAlmostDoneColor
                        }
                    }
                };

                _log.LogDebug($"HIGHLIGHT--New CurrentSession.SessionId: {CurrentSession.SessionId}");
                
                await CurrentSession.Save();

                _log.LogTrace("HIGHLIGHT--Session saved to storage");
            }

            _log.LogInformation("InitializeSession ->");
            return true;
        }

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

            return true;
        }

        protected async Task<bool> CreateConnection()
        {
            _log.LogInformation("-> SignalRHandler.CreateConnection");

            _hostName = _config.GetValue<string>(HostNameKey);
            _hostNameFree = _config.GetValue<string>(HostNameFreeKey);
            _log.LogDebug($"_hostName: {_hostName}");
            _log.LogDebug($"_hostNameFree: {_hostNameFree}");

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

            tcs.SetResult(true);
            return tcs.Task;
        }

        protected virtual void DisplayMessage(string message)
        {
            _log.LogInformation("-> DisplayMessage");
            _log.LogDebug(message);

            CurrentMessage = message;
            RaiseUpdateEvent();
        }

        protected void RaiseUpdateEvent()
        {
            UpdateUi?.Invoke(this, EventArgs.Empty);
        }

        private bool _isOneClockRunning;

        protected void RunClock(StartClockMessage activeClock)
        {
            _log.LogInformation($"HIGHLIGHT---> {nameof(RunClock)}");

            activeClock.IsClockRunning = true;
            activeClock.CurrentBackgroundColor = activeClock.RunningColor;
            Status = $"Clock {activeClock.Label} is running";

            if (_isOneClockRunning)
            {
                _log.LogTrace("HIGHLIGHT--No clocks running");
                return;
            }

            Task.Run(async () =>
            {
                do
                {
                    if (CurrentSession.ClockMessages.Count == 0)
                    {
                        _log.LogTrace("HIGHLIGHT--No clocks found");
                        _isOneClockRunning = false;
                        return;
                    }

                    _isOneClockRunning = false;

                    foreach (var clock in CurrentSession.ClockMessages)
                    {
                        if (clock.IsClockRunning)
                        {
                            _isOneClockRunning = true;

                            var elapsed = DateTime.Now - clock.ServerTime;
                            var remains = clock.CountDown - elapsed;

                            if (remains.TotalSeconds < 0)
                            {
                                clock.IsClockRunning = false;
                                clock.CurrentBackgroundColor = StartClockMessage.DefaultBackgroundColor;
                                clock.ClockDisplay = StartClockMessage.DefaultClockDisplay;
                                Status = $"Countdown finished for {clock.Label}";
                                CountdownFinished?.Invoke(this, EventArgs.Empty);
                                return;
                            }

                            if (Math.Floor(remains.TotalSeconds) <= clock.PayAttention.TotalSeconds)
                            {
                                _log.LogDebug($"ATTENTION Set background to {clock.PayAttentionColor} / {clock.Label}");
                                clock.CurrentBackgroundColor = clock.PayAttentionColor;
                            }

                            if (Math.Floor(remains.TotalSeconds) <= clock.AlmostDone.TotalSeconds)
                            {
                                _log.LogDebug($"ALMOSTDONE Set background to {activeClock.AlmostDoneColor} / {clock.Label}");
                                clock.CurrentBackgroundColor = clock.AlmostDoneColor;
                            }

                            clock.ClockDisplay = remains.ToString(@"hh\:mm\:ss");
                            _log.LogDebug($"{clock.Label}: {clock.ClockDisplay}");
                        }
                    }

                    RaiseUpdateEvent();
                    await Task.Delay(1000);
                }
                while (_isOneClockRunning);
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

        protected virtual void StopLocalClock(string clockId)
        {
            _log.LogInformation("-> StopClock");
            _log.LogDebug($"clockId: {clockId}");

            if (string.IsNullOrEmpty(clockId))
            {
                _log.LogWarning("Empty clockId");
                return;
            }

            var existingClock = CurrentSession.ClockMessages
                .FirstOrDefault(c => c.ClockId == clockId);

            existingClock.CurrentBackgroundColor = "#FFFFFF";
            existingClock.IsClockRunning = false;
            Status = $"Clock {existingClock.Label} was stopped";
        }

        public abstract Task Connect();
    }
}