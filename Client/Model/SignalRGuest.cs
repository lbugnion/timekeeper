using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
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

        public async Task<bool> InitializeGuestInfo()
        {
            _log.LogInformation("HIGHLIGHT---> InitializeGuestInfo");

            GuestInfo = await Guest.GetFromStorage();

            if (GuestInfo == null)
            {
                _log.LogTrace("HIGHLIGHT--GuestInfo is null");
                _log.LogDebug($"HIGHLIGHT--CurrentSession.UserId {CurrentSession.UserId}");
                GuestInfo = new Guest(CurrentSession.UserId);
                await GuestInfo.Save();
            }

            if (GuestInfo.Message.GuestId != CurrentSession.UserId)
            {
                _log.LogTrace($"HIGHLIGHT--Fixing GuestId");
                _log.LogDebug($"HIGHLIGHT--CurrentSession.UserId {CurrentSession.UserId}");
                GuestInfo.Message.GuestId = CurrentSession.UserId;
                await GuestInfo.Save();
            }

            _log.LogDebug($"name: {GuestInfo.Message.DisplayName}");
            _log.LogInformation("InitializeGuestInfo ->");
            return true;
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
                && await InitializeGuestInfo()
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

                    _log.LogTrace($"Name is {GuestInfo.Message.DisplayName}");

                    if (!string.IsNullOrEmpty(GuestInfo.Message.CustomName))
                    {
                        _log.LogTrace($"Sending name {GuestInfo.Message.CustomName}");

                        ok = await AnnounceName();

                        if (!ok)
                        {
                            IsConnected = false;
                            CurrentMessage = "Error";
                        }
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

        public async Task<bool> AnnounceName()
        {
            _log.LogInformation($"-> {nameof(AnnounceName)}");
            _log.LogDebug($"UserId: {CurrentSession.UserId}");
            _log.LogDebug($"GuestId: {GuestInfo.Message.GuestId}");

            var json = JsonConvert.SerializeObject(GuestInfo.Message);
            _log.LogDebug($"json: {json}");

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
                _log.LogInformation($"{nameof(AnnounceName)} ->");
                return false;
            }

            _log.LogInformation($"{nameof(AnnounceName)} ->");
            return true;
        }
    }
}