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
using System.Xml.Serialization;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public abstract class SignalRHandler
    {
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

        public bool IsAnyClockRunning
        {
            get
            {
                return CurrentSession.Clocks.Any(c => c.IsClockRunning);
            }
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
            string sessionId = null,
            string templateName = null,
            bool forceDeleteSession = false)
        {
            _log.LogInformation("-> InitializeSession");
            _log.LogDebug($"sessionId: {sessionId}");
            _log.LogDebug($"HIGHLIGHT--forceDeleteSession: {forceDeleteSession}");

            if (forceDeleteSession)
            {
                await Session.DeleteFromStorage(_log);
            }

            if (!string.IsNullOrEmpty(templateName))
            {
                _log.LogTrace("Checking template");

                var section = _config.GetSection(templateName);
                var config = section.Get<ClockTemplate>();

                _log.LogDebug($"section found: {section != null}");
                _log.LogDebug($"config found: {config != null}");

                CurrentSession = await Session.GetFromStorage(_log);

                if (CurrentSession != null
                    && CurrentSession.Clocks != null
                    && CurrentSession.Clocks.Count != config.CK.Count)
                {
                    _log.LogTrace("CRITICAL--Invalid clock count");
                    // Might be a bug
                    CurrentSession = null;
                }

                if (CurrentSession == null
                    || !CurrentSession.CreatedFromTemplate)
                {
                    if (config != null)
                    {
                        _log.LogDebug($"Found {config.SN}");
                        _log.LogDebug($"Found {config.SessionId}");

                        CurrentSession = new Session
                        {
                            CreatedFromTemplate = true
                        };

                        if (!string.IsNullOrEmpty(config.SN))
                        {
                            CurrentSession.SessionName = config.SN;
                        }

                        if (!string.IsNullOrEmpty(config.SessionId))
                        {
                            CurrentSession.SessionId = config.SessionId;
                            _log.LogDebug($"SessionId {CurrentSession.SessionId}");
                        }

                        foreach (var clockInTemplate in config.CK)
                        {
                            var newClock = new Clock();
                            _log.LogDebug($"HIGHLIGHT--newClock.ClockId before: {newClock.Message.ClockId}");

                            if (clockInTemplate.D)
                            {
                                _log.LogDebug($"Found default clock in template: {clockInTemplate.L}");

                                var defaultClock = CurrentSession.Clocks
                                    .FirstOrDefault(c => c.Message.ClockId == Clock.DefaultClockId);

                                if (defaultClock != null)
                                {
                                    _log.LogTrace("Found default clock in Session");
                                    newClock = defaultClock;
                                }
                            }
                            else
                            {
                                newClock.Message.ClockId = Guid.NewGuid().ToString();
                            }

                            _log.LogDebug($"HIGHLIGHT--newClock.ClockId after: {newClock.Message.ClockId}");

                            if (!string.IsNullOrEmpty(clockInTemplate.L))
                            {
                                newClock.Message.Label = clockInTemplate.L;
                                _log.LogDebug($"Clock label {newClock.Message.Label}");
                            }

                            if (!clockInTemplate.D)
                            {
                                // Not default clock
                                newClock.Message.ClockId = Guid.NewGuid().ToString();
                                _log.LogDebug($"Clock ID changed to {newClock.Message.ClockId}");
                            }

                            if (clockInTemplate.A != null)
                            {
                                if (clockInTemplate.A.T.TotalSeconds > 0)
                                {
                                    // Almost done
                                    newClock.Message.AlmostDone = clockInTemplate.A.T;
                                    _log.LogDebug($"Clock AlmostDone {newClock.Message.AlmostDone}");
                                }

                                if (!string.IsNullOrEmpty(clockInTemplate.A.C))
                                {
                                    // Almost done color
                                    newClock.Message.AlmostDoneColor = clockInTemplate.A.C;

                                    if (!clockInTemplate.A.C.StartsWith("#"))
                                    {
                                        newClock.Message.AlmostDoneColor = "#" + newClock.Message.AlmostDoneColor;
                                    }

                                    _log.LogDebug($"Clock AlmostDoneColor {newClock.Message.AlmostDoneColor}");
                                }
                            }

                            if (clockInTemplate.P != null)
                            {
                                if (clockInTemplate.P.T.TotalSeconds > 0)
                                {
                                    // Pay attention
                                    newClock.Message.PayAttention = clockInTemplate.P.T;
                                    _log.LogDebug($"Clock PayAttention {newClock.Message.PayAttention}");
                                }

                                if (!string.IsNullOrEmpty(clockInTemplate.P.C))
                                {
                                    // Pay attention color
                                    newClock.Message.PayAttentionColor = clockInTemplate.P.C;

                                    if (!clockInTemplate.P.C.StartsWith("#"))
                                    {
                                        newClock.Message.PayAttentionColor = "#" + newClock.Message.PayAttentionColor;
                                    }

                                    _log.LogDebug($"Clock PayAttentionColor {newClock.Message.PayAttentionColor}");
                                }
                            }

                            if (clockInTemplate.C != null)
                            {
                                if (clockInTemplate.C.T.TotalSeconds > 0)
                                {
                                    // Countdown
                                    newClock.Message.CountDown = clockInTemplate.C.T;

                                    _log.LogDebug($"Clock CountDown {newClock.Message.CountDown}");
                                }

                                if (!string.IsNullOrEmpty(clockInTemplate.C.C))
                                {
                                    _log.LogTrace("Checking countdown color");

                                    // Countdown color
                                    newClock.Message.RunningColor = clockInTemplate.C.C;

                                    _log.LogTrace("Checking countdown color");

                                    if (!clockInTemplate.C.C.StartsWith("#"))
                                    {
                                        newClock.Message.RunningColor = "#" + newClock.Message.RunningColor;
                                    }

                                    _log.LogDebug($"Clock RunningColor {newClock.Message.RunningColor}");
                                }
                            }

                            CurrentSession.Clocks.Add(newClock);
                        }

                        await CurrentSession.Save(_log);
                    }
                }
            }

            if (string.IsNullOrEmpty(sessionId)
                && CurrentSession == null)
            {
                CurrentSession = await Session.GetFromStorage(_log);

                if (CurrentSession != null
                    && CurrentSession.Clocks.Count == 0)
                {
                    CurrentSession.Clocks.Add(new Clock());
                }
            }

            //if (forceDeleteSession)
            //{
            //    CurrentSession = null;
            //}

            if (CurrentSession == null)
            {
                _log.LogTrace("CurrentSession is null");

                CurrentSession = new Session();
                CurrentSession.Clocks.Add(new Clock());

                if (!string.IsNullOrEmpty(sessionId))
                {
                    CurrentSession.SessionId = sessionId;
                }

                _log.LogDebug($"New CurrentSession.SessionId: {CurrentSession.SessionId}");

                _log.LogTrace("Ready to save");
                await CurrentSession.Save(_log);
                _log.LogTrace("Saved");

                _log.LogTrace("Session saved to storage");
            }

            _log.LogTrace("003");

            foreach (var clock in CurrentSession.Clocks)
            {
                _log.LogDebug($"Setting clock {clock.Message.Label}");

                clock.IsStartDisabled = true;
                clock.IsStopDisabled = true;
                clock.IsConfigDisabled = true;
                clock.IsDeleteDisabled = true;
                clock.IsClockRunning = false;
                clock.ClockDisplay = clock.Message.CountDown.ToString("c");
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

        protected void RunClock(Clock activeClock)
        {
            _log.LogInformation($"-> {nameof(RunClock)}");

            activeClock.CurrentBackgroundColor = activeClock.Message.RunningColor;
            Status = $"Clock {activeClock.Message.Label} is running";
            RaiseUpdateEvent();

            _log.LogDebug($"ClockId: {activeClock.Message.ClockId}");
            _log.LogDebug($"CurrentBackgroundColor: {activeClock.CurrentBackgroundColor}");
            _log.LogDebug($"Label: {activeClock.Message.Label}");

            if (IsAnyClockRunning)
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
                    if (CurrentSession.Clocks.Count == 0)
                    {
                        _log.LogTrace("No clocks found");
                        return;
                    }

                    foreach (var clock in CurrentSession.Clocks)
                    {
                        if (clock.IsClockRunning)
                        {
                            var elapsed = DateTime.Now - clock.Message.ServerTime;
                            var remains = clock.Message.CountDown - elapsed;

                            if (remains.TotalSeconds <= 0)
                            {
                                _log.LogTrace("Countdown finished");
                                clock.IsClockRunning = false;
                                clock.ClockDisplay = Clock.DefaultClockDisplay;
                                clock.CurrentBackgroundColor = clock.Message.AlmostDoneColor;
                                CurrentMessage = $"{clock.Message.Label} is over!!!";
                                Status = $"Countdown finished for {clock.Message.Label}";
                                clock.RaiseCountdownFinished();
                                continue;
                            }

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
                    await Task.Delay(1000);
                }
                while (IsAnyClockRunning);
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

        protected virtual void DeleteLocalClock(string clockId)
        {
            if (clockId == Clock.DefaultClockId)
            {
                return;
            }

            var clock = CurrentSession.Clocks.FirstOrDefault(c => c.Message.ClockId == clockId);

            if (clock == null
                || clock.IsClockRunning)
            {
                return;
            }

            StopLocalClock(clockId);
            CurrentSession.Clocks.Remove(clock);
        }

        protected virtual void StopLocalClock(string clockId)
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

            existingClock.CurrentBackgroundColor = Clock.DefaultBackgroundColor;
            existingClock.IsClockRunning = false;
            existingClock.ResetDisplay();

            Status = $"Clock {existingClock.Message.Label} was stopped";
            RaiseUpdateEvent();
            _log.LogInformation($"{nameof(StopLocalClock)} ->");
        }

        public abstract Task Connect(
            string templateName = null, 
            bool forceDeleteSession = false);
    }
}