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
using TimeKeeperApi.DataModel;

namespace TimekeeperClient.Model
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
            _log.LogInformation("HIGHLIGHT---> SignalRHost.ReceiveGuestMessage");
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

            if (!success)
            {
                _log.LogWarning($"GuestId is not a GUID");
                return;
            }

            var existingGuest = ConnectedGuests.FirstOrDefault(g => g.GuestId == messageGuest.GuestId);

            if (existingGuest == null)
            {
                _log.LogTrace("No existing guest found");

                ConnectedGuests.Add(new GuestMessage
                {
                    GuestId = messageGuest.GuestId,
                    CustomName = messageGuest.CustomName
                });

                _log.LogTrace("Added");
            }
            else
            {
                _log.LogDebug($"Existing guest found: Old name {existingGuest.DisplayName}");
                existingGuest.CustomName = messageGuest.CustomName;
                _log.LogDebug($"Existing guest found: New name {existingGuest.DisplayName}");
            }

            RaiseUpdateEvent();
            _log.LogInformation("HIGHLIGHT--SignalRHost.ReceiveGuestMessage ->");
        }

        public async Task SendMessage()
        {
            _log.LogInformation("-> SendMessage");

            if (string.IsNullOrEmpty(InputMessage))
            {
                return;
            }

            try
            {
                CurrentMessage = InputMessage;
                var content = new StringContent(InputMessage);

                var functionKey = _config.GetValue<string>(SendMessageKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var sendMessageUrl = $"{_hostName}/send";
                _log.LogDebug($"sendMessageUrl: {sendMessageUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, sendMessageUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
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
        }

        public async Task StartClock()
        {
            if (IsClockRunning)
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
                var activeClock = CurrentSession.ClockMessage.GetFresh();

                var json = JsonConvert.SerializeObject(activeClock);
                var content = new StringContent(json);

                _log.LogDebug($"json: {json}");

                var functionKey = _config.GetValue<string>(StartClockKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var startClockUrl = $"{_hostName}/start";
                _log.LogDebug($"startClockUrl: {startClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, startClockUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    RunClock(activeClock);
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

        public async Task StopAllClocks()
        {
            _log.LogInformation("-> StopAllClocks");

            StopClock(null);

            // Notify clients

            try
            {
                var functionKey = _config.GetValue<string>(StopClockKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var stopClockUrl = $"{_hostName}/stop";
                _log.LogDebug($"stopClockUrl: {stopClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Get, stopClockUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
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