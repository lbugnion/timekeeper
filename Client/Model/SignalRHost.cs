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

            await CurrentSession.DeleteFromStorage();
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

            clock.IsStartDisabled = false;
            clock.IsStopDisabled = true;
            clock.IsConfigDisabled = false;
            clock.IsDeleteDisabled = false;

            if (!CurrentSession.Clocks.Any(c => c.IsClockRunning))
            {
                IsDeleteSessionDisabled = false;
            }

            RaiseUpdateEvent();
        }

        protected override void DisplayMessage(string message)
        {
            base.DisplayMessage(message);
            Status = "Message sent";
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRHost.Connect");

            IsBusy = true;

            IsSendMessageDisabled = true;
            IsDeleteSessionDisabled = true;
            IsCreateNewSessionDisabled = true;

            var ok = await InitializeSession()
                && await CreateConnection();

            if (ok)
            {
                _connection.On<string>(Constants.GuestToHostMessageName, ReceiveGuestMessage);
                _connection.On<string>(Constants.ConnectMessage, ReceiveConnectMessage);
                _connection.On<string>(Constants.DisconnectMessage, ReceiveDisconnectMessage);

                ok = await StartConnection();

                if (ok)
                {
                    _log.LogTrace("OK");

                    IsConnected = true;

                    foreach (var clock in CurrentSession.Clocks)
                    {
                        clock.IsStartDisabled = false;
                        clock.IsStopDisabled = true;
                        clock.IsConfigDisabled = false;
                        clock.IsDeleteDisabled = false;
                    }

                    IsSendMessageDisabled = false;
                    IsDeleteSessionDisabled = false;
                    IsCreateNewSessionDisabled = true;
                    CurrentMessage = "Ready";
                }
                else
                {
                    _log.LogTrace("NOT OK");

                    IsConnected = false;

                    foreach (var clock in CurrentSession.Clocks)
                    {
                        clock.IsStartDisabled = true;
                        clock.IsStopDisabled = false;
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
                _log.LogTrace("NOT OK");

                IsConnected = false;

                foreach (var clock in CurrentSession.Clocks)
                {
                    clock.IsStartDisabled = true;
                    clock.IsStopDisabled = false;
                    clock.IsConfigDisabled = true;
                    clock.IsDeleteDisabled = true;
                }

                IsSendMessageDisabled = true;
                IsDeleteSessionDisabled = false;
                IsCreateNewSessionDisabled = true;
                CurrentMessage = "Error";
            }

            IsBusy = false;
            _log.LogInformation("SignalRHost.Connect ->");
        }

        public void ReceiveGuestMessage(string json)
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

        public void ReceiveConnectMessage(string guestId)
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
                _log.LogWarning("Found existing guest, nothing to do");
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
            }
            catch (Exception ex)
            {
                _log.LogError($"Cannot send message: {ex.Message}");
                ErrorStatus = "Error sending message";
            }

            _log.LogInformation($"{nameof(SendMessage)} ->");
        }

        public async Task StartClock(Clock clock)
        {
            if (clock == null
                || clock.IsClockRunning)
            {
                return;
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
                clock.Reset();

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
                }
                else
                {
                    ErrorStatus = "Unable to communicate with clients";
                }
            }
            catch
            {
                CurrentMessage = "Unable to communicate with clients";
            }
        }

        public async Task StopClock(Clock clock)
        {
            _log.LogInformation("-> StopClock");

            StopLocalClock(clock.Message.ClockId);

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
            IsCreateNewSessionDisabled = isOneClockRunning;
            _log.LogInformation("StopClock ->");
        }

        public IList<GuestMessage> ConnectedGuests
        {
            get;
            private set;
        }

        public async Task DeleteClock(Clock clock)
        {
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
                    await CurrentSession.Save();
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
                CurrentSession = CurrentSession,
                CurrentClock = clock
            };

            Program.ClockToConfigure = param;
            return true;
        }
    }
}