using Blazored.LocalStorage;
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
    public class SignalRHost : SignalRHandler
    {
        public string InputMessage
        {
            get;
            set;
        }

        public bool IsSendMessageDisabled
        {
            get;
            private set;
        }

        public bool IsDeleteSessionWarningVisible
        {
            get;
            private set;
        }

        public SignalRHost(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http) : base(config, localStorage, log, http)
        {
            ConnectedGuests = new List<GuestMessage>();
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

            foreach (var clock in CurrentSession.Clocks)
            {
                clock.CountdownFinished -= ClockCountdownFinished;
            }

            await HostSession.DeleteFromStorage(_log);
            CurrentSession = null;
            _log.LogTrace("CurrentSession is deleted");

            IsDeleteSessionDisabled = true;
            IsCreateNewSessionDisabled = false;
            Status = "Disconnected";

            _log.LogInformation("DoDeleteSession ->");
        }

        public void CancelDeleteSession()
        {
            _log.LogInformation("-> CancelDeleteSession");
            IsDeleteSessionDisabled = false;
            IsDeleteSessionWarningVisible = false;
        }

        public void DeleteSession()
        {
            _log.LogInformation("-> DeleteSession");
            IsDeleteSessionDisabled = true;
            IsDeleteSessionWarningVisible = true;
        }

        private void ClockCountdownFinished(object sender, EventArgs e)
        {
            _log.LogInformation("-> ClockCountdownFinished");

            var clock = sender as Clock;

            if (clock == null)
            {
                return;
            }

            var isAnyClockRunning = IsAnyClockRunning;

            _log.LogDebug($"IsAnyClockRunning {isAnyClockRunning}");

            clock.IsStartDisabled = false;
            clock.IsStopDisabled = true;
            clock.IsDeleteDisabled = false;

            if (isAnyClockRunning)
            {
                foreach (var anyClock in CurrentSession.Clocks)
                {
                    anyClock.IsConfigDisabled = true;
                }
            }
            else
            {
                IsDeleteSessionDisabled = false;

                foreach (var anyClock in CurrentSession.Clocks)
                {
                    anyClock.IsConfigDisabled = false;
                }
            }

            RaiseUpdateEvent();
        }

        public override async Task Connect(
            string templateName = null, 
            bool forceDeleteSession = false)
        {
            _log.LogInformation("HIGHLIGHT---> SignalRHost.Connect");

            IsBusy = true;

            IsSendMessageDisabled = true;
            IsDeleteSessionDisabled = true;
            IsCreateNewSessionDisabled = true;

            var ok = await InitializeSession(templateName: templateName, forceDeleteSession)
                && await CreateConnection();

            if (ok)
            {
                _connection.On<string>(Constants.GuestToHostMessageName, ReceiveGuestMessage);
                _connection.On<string>(Constants.ConnectMessage, ReceiveConnectMessage);
                _connection.On<string>(Constants.DisconnectMessage, ReceiveDisconnectMessage);

                ok = await StartConnection();

                if (ok)
                {
                    _log.LogTrace("CreateConnection and StartConnection OK");

                    IsConnected = true;

                    foreach (var clock in CurrentSession.Clocks)
                    {


                        clock.IsStartDisabled = false;
                        clock.IsStopDisabled = true;
                        clock.IsConfigDisabled = false;
                        clock.IsDeleteDisabled = false;

                        if (clock.Message.ServerTime + clock.Message.CountDown > DateTime.Now)
                        {
                            _log.LogDebug($"Clock {clock.Message.Label} is still active");
                            await StartClock(clock, false);
                        }
                    }

                    IsSendMessageDisabled = false;
                    IsDeleteSessionDisabled = false;
                    IsCreateNewSessionDisabled = true;
                    CurrentMessage = "Ready";
                }
                else
                {
                    _log.LogTrace("StartConnection NOT OK");

                    IsConnected = false;

                    foreach (var clock in CurrentSession.Clocks)
                    {
                        clock.IsStartDisabled = true;
                        clock.IsStopDisabled = true;
                        clock.IsConfigDisabled = true;
                        clock.IsDeleteDisabled = true;
                    }

                    IsSendMessageDisabled = true;
                    IsDeleteSessionDisabled = false;
                    IsCreateNewSessionDisabled = true;
                    CurrentMessage = "Error";
                }
            }
            else
            {
                _log.LogTrace("CreateConnection NOT OK");

                IsConnected = false;

                foreach (var clock in CurrentSession.Clocks)
                {
                    clock.IsStartDisabled = true;
                    clock.IsStopDisabled = true;
                    clock.IsConfigDisabled = true;
                    clock.IsDeleteDisabled = true;
                }

                IsSendMessageDisabled = true;
                IsDeleteSessionDisabled = false;
                IsCreateNewSessionDisabled = true;
                CurrentMessage = "Error";
            }

            IsBusy = false;
            Status = "Connected, your guests will only see clocks when you start them!";
            _log.LogInformation("SignalRHost.Connect ->");
        }

        public async Task ReceiveGuestMessage(string json)
        {
            _log.LogInformation($"-> SignalRHost.{nameof(ReceiveGuestMessage)}");
            _log.LogDebug(json);

            var messageGuest = JsonConvert.DeserializeObject<GuestMessage>(json);

            _log.LogDebug($"GuestId: {messageGuest.GuestId}");

            if (messageGuest == null
                || string.IsNullOrEmpty(messageGuest.GuestId))
            {
                _log.LogWarning($"No GuestId found");
                return;
            }

            var success = Guid.TryParse(messageGuest.GuestId, out Guid guestGuid);

            if (!success
                || guestGuid == Guid.Empty)
            {
                _log.LogWarning($"GuestId is not a GUID");
                return;
            }

            var existingGuest = ConnectedGuests.FirstOrDefault(g => g.GuestId == messageGuest.GuestId);

            if (existingGuest == null)
            {
                _log.LogWarning("No existing guest found");
            }
            else
            {
                _log.LogDebug($"Existing guest found: Old name {existingGuest.DisplayName}");
                existingGuest.CustomName = messageGuest.CustomName;
                _log.LogDebug($"Existing guest found: New name {existingGuest.DisplayName}");
            }

            RaiseUpdateEvent();
            _log.LogInformation($"SignalRHost.{nameof(ReceiveGuestMessage)} ->");
        }

        public void ReceiveDisconnectMessage(string guestId)
        {
            _log.LogInformation($"-> SignalRHost.{nameof(ReceiveDisconnectMessage)}");
            _log.LogDebug($"{nameof(guestId)} {guestId}");
            _log.LogDebug($"UserId in CurrentSession: {CurrentSession.UserId}");

            var success = Guid.TryParse(guestId, out Guid guestGuid);

            if (!success
                || guestGuid == Guid.Empty)
            {
                _log.LogWarning($"GuestId is not a GUID");
                return;
            }

            var existingGuest = ConnectedGuests.FirstOrDefault(g => g.GuestId == guestId);

            if (existingGuest == null)
            {
                _log.LogWarning("No existing guest found");
                return;
            }

            ConnectedGuests.Remove(existingGuest);
            RaiseUpdateEvent();
            _log.LogInformation($"SignalRHost.{nameof(ReceiveDisconnectMessage)} ->");
        }

        public async Task ReceiveConnectMessage(string guestId)
        {
            _log.LogInformation($"-> SignalRHost.{nameof(ReceiveConnectMessage)}");
            _log.LogDebug($"{nameof(guestId)} {guestId}");
            _log.LogDebug($"UserId in CurrentSession: {CurrentSession.UserId}");

            var success = Guid.TryParse(guestId, out Guid guestGuid);

            if (!success
                || guestGuid == Guid.Empty)
            {
                _log.LogWarning($"GuestId is not a GUID");
                return;
            }

            var existingGuest = ConnectedGuests.FirstOrDefault(g => g.GuestId == guestId);

            if (existingGuest != null)
            {
                _log.LogWarning("Found existing guest, refresh clock just to be sure");

                if (IsAnyClockRunning)
                {
                    await StartAllClocks(false);
                }

                return;
            }

            if (guestId == CurrentSession.UserId)
            {
                _log.LogWarning($"Self connect received");
                return;
            }

            ConnectedGuests.Add(new GuestMessage
            {
                GuestId = guestId
            });

            RaiseUpdateEvent();

            if (IsAnyClockRunning)
            {
                await StartAllClocks(false);
            }

            _log.LogInformation($"SignalRHost.{nameof(ReceiveConnectMessage)} ->");
        }

        public async Task SendMessage()
        {
            _log.LogInformation($"-> {nameof(SendMessage)}");

            if (string.IsNullOrEmpty(InputMessage))
            {
                return;
            }

            try
            {
                CurrentMessage = InputMessage;
                var content = new StringContent(InputMessage);

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
            foreach (var clock in CurrentSession.Clocks)
            {
                await StartClock(clock, startFresh);
            }
        }

        public async Task StartClock(
            Clock clock, 
            bool startFresh)
        {
            if (startFresh)
            {
                if (clock == null
                    || clock.IsClockRunning)
                {
                    return;
                }
            }

            _log.LogInformation("-> SignalRHost.StartClock");

            clock.IsStartDisabled = true;
            clock.IsStopDisabled = false;
            clock.IsConfigDisabled = true;
            clock.IsDeleteDisabled = true;
            clock.CountdownFinished += ClockCountdownFinished;
            IsDeleteSessionDisabled = true;
            IsCreateNewSessionDisabled = true;

            try
            {
                if (startFresh)
                {
                    clock.Reset();

                    // Save so that we can restart the clock if the page is reloaded
                    await CurrentSession.Save(_log);
                }

                var json = JsonConvert.SerializeObject(clock.Message);
                var content = new StringContent(json);

                _log.LogDebug($"json: {json}");

                var startClockUrl = $"{_hostName}/start";
                _log.LogDebug($"startClockUrl: {startClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, startClockUrl);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    RunClock(clock);

                    foreach (var anyClock in CurrentSession.Clocks)
                    {
                        anyClock.IsConfigDisabled = true;
                    }
                }
                else
                {
                    ErrorStatus = "Unable to communicate with clients";
                    IsDeleteSessionDisabled = false;
                }
            }
            catch
            {
                CurrentMessage = "Unable to communicate with clients";
                IsDeleteSessionDisabled = false;
            }
        }

        public async Task StopClock(Clock clock)
        {
            _log.LogInformation("-> StopClock");

            await StopLocalClock(clock.Message.ClockId);

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

            clock.IsStartDisabled = false;
            clock.IsStopDisabled = true;
            clock.IsConfigDisabled = false;
            clock.IsDeleteDisabled = false;
            clock.CountdownFinished -= ClockCountdownFinished;

            var isOneClockRunning = CurrentSession.Clocks.Any(c => c.IsClockRunning);

            IsDeleteSessionDisabled = isOneClockRunning;
            IsCreateNewSessionDisabled = !isOneClockRunning;
            _log.LogInformation("StopClock ->");
        }

        public IList<GuestMessage> ConnectedGuests
        {
            get;
            private set;
        }

        public async Task DeleteClock(Clock clock)
        {
            clock.CountdownFinished -= ClockCountdownFinished;
            DeleteLocalClock(clock.Message.ClockId);

            // Notify clients

            try
            {
                var deleteClockUrl = $"{_hostName}/delete";
                _log.LogDebug($"deleteClockUrl: {deleteClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, deleteClockUrl);
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

            clock.IsStartDisabled = false;
            clock.IsStopDisabled = true;
            clock.IsConfigDisabled = false;
            clock.IsDeleteDisabled = false;
            clock.CountdownFinished -= ClockCountdownFinished;

            var isOneClockRunning = CurrentSession.Clocks.Any(c => c.IsClockRunning);

            IsDeleteSessionDisabled = isOneClockRunning;
            IsCreateNewSessionDisabled = isOneClockRunning;
            _log.LogInformation("DeleteClock ->");

        }

        public async Task AddClockAfter(string clockId)
        {
            var previousClock = CurrentSession.Clocks.FirstOrDefault(c => c.Message.ClockId == clockId);

            if (previousClock != null)
            {
                var index = CurrentSession.Clocks.IndexOf(previousClock);

                if (index > -1)
                {
                    var newClock = new Clock();
                    newClock.Message.ClockId = Guid.NewGuid().ToString();
                    CurrentSession.Clocks.Insert(index + 1, newClock);
                    await CurrentSession.Save(_log);
                }
            }
        }

        public bool PrepareClockToConfigure(string clockId)
        {
            var clock = CurrentSession.Clocks.FirstOrDefault(c => c.Message.ClockId == clockId);

            if (clock == null)
            {
                return false;
            }

            var param = new ConfigureClock
            {
                CurrentSession = (HostSession)CurrentSession,
                CurrentClock = clock
            };

            Program.ClockToConfigure = param;
            return true;
        }

        public async Task<bool> InitializeSession(
            string templateName = null,
            bool forceDeleteSession = false)
        {
            _log.LogInformation("HIGHLIGHT---> SignalRHost.InitializeSession");
            _log.LogDebug($"forceDeleteSession: {forceDeleteSession}");

            if (forceDeleteSession)
            {
                await HostSession.DeleteFromStorage(_log);
            }

            CurrentSession = await HostSession.GetFromStorage(_log);

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
                        CurrentSession = new HostSession
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

            if (CurrentSession == null)
            {
                _log.LogTrace("HIGHLIGHT--CurrentSession is null");

                CurrentSession = new HostSession();
                CurrentSession.Clocks.Add(new Clock());

                _log.LogDebug($"New CurrentSession.SessionId: {CurrentSession.SessionId}");
                await CurrentSession.Save(_log);
                _log.LogTrace("Session saved to storage");
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
            _log.LogInformation("HIGHLIGHT--SignalRHost.InitializeSession ->");
            return true;
        }

    }
}