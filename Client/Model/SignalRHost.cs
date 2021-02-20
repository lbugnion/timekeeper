using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
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

        public bool IsStartDisabled
        {
            get;
            private set;
        }

        public bool IsStopDisabled
        {
            get;
            private set;
        }

        public bool IsConfigureSessionDisabled
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
            IsStopDisabled = true;
            base.CountdownFinished += SignalRHostCountdownFinished;
            ConnectedGuests = new List<GuestMessage>();
        }

        public override async Task DeleteSession()
        {
            await base.DeleteSession();
            IsConfigureSessionDisabled = true;
        }

        private void SignalRHostCountdownFinished(object sender, EventArgs e)
        {
            _log.LogInformation("-> SignalRHostCountdownFinished");
            IsStartDisabled = false;
            IsStopDisabled = true;
            IsConfigureSessionDisabled = false;
            IsDeleteSessionDisabled = false;
            IsCreateNewSessionDisabled = true;
            RaiseUpdateEvent();
        }

        protected override void DisplayMessage(string message)
        {
            base.DisplayMessage(message);
            Status = "Message sent";
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRHost.ConnectToServer");

            IsBusy = true;
            IsStartDisabled = true;
            IsStopDisabled = true;
            IsSendMessageDisabled = true;
            IsDeleteSessionDisabled = true;
            IsCreateNewSessionDisabled = true;
            IsConfigureSessionDisabled = true;

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
                    IsStartDisabled = false;
                    IsStopDisabled = true;
                    IsSendMessageDisabled = false;
                    IsDeleteSessionDisabled = false;
                    IsCreateNewSessionDisabled = true;
                    IsConfigureSessionDisabled = false;
                    CurrentMessage = "Ready";
                }
                else
                {
                    _log.LogTrace("NOT OK");

                    IsConnected = false;
                    IsStartDisabled = true;
                    IsStopDisabled = true;
                    IsSendMessageDisabled = true;
                    IsDeleteSessionDisabled = false;
                    IsCreateNewSessionDisabled = true;
                    IsConfigureSessionDisabled = false;
                    CurrentMessage = "Error";
                }
            }
            else
            {
                _log.LogTrace("NOT OK");

                IsConnected = false;
                IsStartDisabled = true;
                IsStopDisabled = true;
                IsSendMessageDisabled = true;
                IsDeleteSessionDisabled = false;
                IsCreateNewSessionDisabled = true;
                IsConfigureSessionDisabled = false;
                CurrentMessage = "Error";
            }

            IsBusy = false;
            _log.LogInformation("SignalRHost.ConnectToServer ->");
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

        public async Task StartClock(string clockId)
        {
            var clock = CurrentSession.ClockMessages
                .FirstOrDefault(c => c.ClockId == clockId);

            if (clock == null
                || clock.IsClockRunning)
            {
                return;
            }

            _log.LogInformation("-> SignalRHost.StartClock");

            IsStartDisabled = true;
            IsStopDisabled = false;
            IsConfigureSessionDisabled = true;
            IsDeleteSessionDisabled = true;
            IsCreateNewSessionDisabled = true;

            try
            {
                clock.Reset();

                var json = JsonConvert.SerializeObject(clock);
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

        public async Task StopClock(string clockId)
        {
            _log.LogInformation("-> StopClock");

            StopLocalClock(clockId);

            // Notify clients

            try
            {
                var stopClockUrl = $"{_hostName}/stop";
                _log.LogDebug($"stopClockUrl: {stopClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, stopClockUrl);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

                var content = new StringContent(clockId);
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

            IsStartDisabled = false;
            IsStopDisabled = true;
            IsConfigureSessionDisabled = false;
            IsDeleteSessionDisabled = false;
            IsCreateNewSessionDisabled = true;
            _log.LogInformation("StopAllClocks ->");
        }

        public IList<GuestMessage> ConnectedGuests
        {
            get;
            private set;
        }
    }
}