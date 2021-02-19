using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using TimeKeeperApi.DataModel;

namespace TimekeeperClient.Model
{
    public class SignalRGuest : SignalRHandler
    {
        private string _session;

        public Guest GuestInfo
        {
            get;
            private set;
        }

        public SignalRGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string session) : base(config, localStorage, log, http)
        {
            _log.LogInformation("> SignalRGuest()");

            _session = session;
            Guest.SetLocalStorage(localStorage, log);
        }

        public async Task InitializeGuestInfo()
        {
            _log.LogInformation("> InitializeGuestInfo");

            GuestInfo = await Guest.GetFromStorage();

            if (GuestInfo == null)
            {
                _log.LogTrace("GuestInfo is null");
                GuestInfo = new Guest();
            }

            _log.LogDebug($"name: {GuestInfo.Message.DisplayName}");
            _log.LogInformation("InitializeGuestInfo ->");
        }

        private void ReceiveStartClock(string message)
        {
            _log.LogInformation("-> SignalRGuest.ReceiveStartClock");
            _log.LogDebug($"message: {message}");

            CurrentSession.ClockMessage = JsonConvert.DeserializeObject<StartClockMessage>(message);

            _log.LogDebug($"CountDown: {CurrentSession.ClockMessage.CountDown}");
            _log.LogDebug($"Red: {CurrentSession.ClockMessage.AlmostDone}");
            _log.LogDebug($"ServerTime: {CurrentSession.ClockMessage.ServerTime}");
            _log.LogDebug($"Yellow: {CurrentSession.ClockMessage.PayAttention}");

            RunClock(CurrentSession.ClockMessage);
            Status = "Clock started";
            _log.LogInformation("SignalRGuest.ReceiveStartClock ->");
        }

        protected override void DisplayMessage(string message)
        {
            base.DisplayMessage(message);
            Status = "Received host message";
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRGuest.Connect");

            IsBusy = true;

            var ok = await InitializeSession(_session)
                && await CreateConnection();

            if (ok)
            {
                _connection.On<string>(Constants.StartClockMessageName, ReceiveStartClock);
                _connection.On<string>(Constants.HostToGuestMessageName, DisplayMessage);
                _connection.On<object>(Constants.StopClockMessage, StopClock);

                ok = await StartConnection();

                if (ok)
                {
                    IsConnected = true;
                    CurrentMessage = "Ready";
                    ok = await Announce();

                    if (!ok)
                    {
                        IsConnected = false;
                        CurrentMessage = "Error";
                    }
                }
                else
                {
                    IsConnected = false;
                    CurrentMessage = "Error";
                }
            }
            else
            {
                IsConnected = false;
                CurrentMessage = "Error";
            }

            IsBusy = false;
            _log.LogInformation("SignalRGuest.Connect ->");
        }

        public async Task<bool> Announce(bool dispose = false)
        {
            _log.LogInformation("HIGHLIGHT---> Announce");

            GuestInfo.Message.Disconnecting = dispose;

            var json = JsonConvert.SerializeObject(GuestInfo.Message);
            _log.LogDebug($"json: {json}");

            var content = new StringContent(json);

            var functionKey = _config.GetValue<string>(AnnounceGuestKeyKey);
            _log.LogDebug($"functionKey: {functionKey}");

            var announceUrl = $"{_hostName}/announce";
            _log.LogDebug($"announceUrl: {announceUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, announceUrl);
            httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
            httpRequest.Content = content;

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Cannot send message: {response.ReasonPhrase}");
                _log.LogInformation("HIGHLIGHT--Announce ->");
                return false;
            }

            _log.LogInformation("HIGHLIGHT--Announce ->");
            return true;
        }
    }
}