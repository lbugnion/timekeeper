﻿using Blazored.LocalStorage;
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
            _log.LogDebug($"forceDeleteSession: {forceDeleteSession}");

            if (forceDeleteSession)
            {
                await Session.DeleteFromStorage(_log);
            }

            CurrentSession = await Session.GetFromStorage(_log);

            // TODO Clean this up, this code is only for the Host

            if (!string.IsNullOrEmpty(templateName))
            {
                _log.LogTrace("Checking template");

                var section = _config.GetSection(templateName);
                var config = section.Get<ClockTemplate>();

                _log.LogDebug($"section found: {section != null}");
                _log.LogDebug($"config found: {config != null}");

                if (config != null)
                {
                    _log.LogDebug($"Found {config.SessionName}");
                    _log.LogDebug($"Found {config.SessionId}");

                    if (!string.IsNullOrEmpty(config.SessionId))
                    {
                        CurrentSession = new Session
                        {
                            CreatedFromTemplate = true,
                            SessionId = config.SessionId
                        };

                        if (!string.IsNullOrEmpty(config.SessionName))
                        {
                            CurrentSession.SessionName = config.SessionName;
                        }

                       foreach (var clockInTemplate in config.Clocks)
                        {
                            var newClock = new Clock();
                            _log.LogDebug($"newClock.ClockId: {newClock.Message.ClockId}");

                            if (!string.IsNullOrEmpty(clockInTemplate.Label))
                            {
                                newClock.Message.Label = clockInTemplate.Label;
                                _log.LogDebug($"Clock label {newClock.Message.Label}");
                            }

                            if (clockInTemplate.AlmostDone != null)
                            {
                                if (clockInTemplate.AlmostDone.Time.TotalSeconds > 0)
                                {
                                    // Almost done
                                    newClock.Message.AlmostDone = clockInTemplate.AlmostDone.Time;
                                    _log.LogDebug($"Clock AlmostDone {newClock.Message.AlmostDone}");
                                }

                                if (!string.IsNullOrEmpty(clockInTemplate.AlmostDone.Color))
                                {
                                    // Almost done color
                                    newClock.Message.AlmostDoneColor = clockInTemplate.AlmostDone.Color;

                                    if (!clockInTemplate.AlmostDone.Color.StartsWith("#"))
                                    {
                                        newClock.Message.AlmostDoneColor = "#" + newClock.Message.AlmostDoneColor;
                                    }

                                    _log.LogDebug($"Clock AlmostDoneColor {newClock.Message.AlmostDoneColor}");
                                }
                            }

                            if (clockInTemplate.PayAttention != null)
                            {
                                if (clockInTemplate.PayAttention.Time.TotalSeconds > 0)
                                {
                                    // Pay attention
                                    newClock.Message.PayAttention = clockInTemplate.PayAttention.Time;
                                    _log.LogDebug($"Clock PayAttention {newClock.Message.PayAttention}");
                                }

                                if (!string.IsNullOrEmpty(clockInTemplate.PayAttention.Color))
                                {
                                    // Pay attention color
                                    newClock.Message.PayAttentionColor = clockInTemplate.PayAttention.Color;

                                    if (!clockInTemplate.PayAttention.Color.StartsWith("#"))
                                    {
                                        newClock.Message.PayAttentionColor = "#" + newClock.Message.PayAttentionColor;
                                    }

                                    _log.LogDebug($"Clock PayAttentionColor {newClock.Message.PayAttentionColor}");
                                }
                            }

                            if (clockInTemplate.Countdown != null)
                            {
                                if (clockInTemplate.Countdown.Time.TotalSeconds > 0)
                                {
                                    // Countdown
                                    newClock.Message.CountDown = clockInTemplate.Countdown.Time;

                                    _log.LogDebug($"Clock CountDown {newClock.Message.CountDown}");
                                }

                                if (!string.IsNullOrEmpty(clockInTemplate.Countdown.Color))
                                {
                                    _log.LogTrace("Checking countdown color");

                                    // Countdown color
                                    newClock.Message.RunningColor = clockInTemplate.Countdown.Color;

                                    _log.LogTrace("Checking countdown color");

                                    if (!clockInTemplate.Countdown.Color.StartsWith("#"))
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
                    else
                    {
                        _log.LogError($"No session ID in template, ignoring");
                    }
                }
            }

            if (string.IsNullOrEmpty(sessionId)
                && CurrentSession == null)
            {
                // This can happen if the Guest was reloaded while a session is active
                _log.LogTrace("sessionId is set and CurrentSession is null");
                CurrentSession = await Session.GetFromStorage(_log);
            }

            if (CurrentSession == null)
            {
                _log.LogTrace("CurrentSession is null");

                CurrentSession = new Session();

                if (string.IsNullOrEmpty(sessionId))
                {
                    // This is a host
                    CurrentSession.Clocks.Add(new Clock());
                }

                _log.LogDebug($"New CurrentSession.SessionId: {CurrentSession.SessionId}");
                _log.LogDebug($"Clock count: {CurrentSession.Clocks.Count}");

                await CurrentSession.Save(_log);
                _log.LogTrace("Session saved to storage");
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                _log.LogTrace("Setting guest session ID");

                // This is a Guest
                CurrentSession.SessionId = sessionId;
                CurrentSession.Clocks = new List<Clock>();
            }

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

            _log.LogDebug($"UserID {CurrentSession.UserId}");
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

            _log.LogInformation("SignalRHandler.StartConnection ->");
            return true;
        }

        protected virtual void DeleteLocalClock(string clockId)
        {
            _log.LogInformation("-> DeleteLocalClock");

            var clock = CurrentSession.Clocks.FirstOrDefault(c => c.Message.ClockId == clockId);

            if (clock == null
                || clock.IsClockRunning)
            {
                _log.LogTrace($"HIGHLIGHT--No clock found with id {clockId}");
                return;
            }

            StopLocalClock(clockId);

            _log.LogDebug("HIGHLIGHT--Clock stopped, removing");

            CurrentSession.Clocks.Remove(clock);
            RaiseUpdateEvent();

            _log.LogDebug("HIGHLIGHT--Removed");

            _log.LogDebug($"HIGHLIGHT--Remaining clocks: {CurrentSession.Clocks.Count}");
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