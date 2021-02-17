using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using TimeKeeperApi.DataModel;

namespace TimekeeperClient.Model
{
    public class SignalRGuest : SignalRHandler
    {
        private string _session;

        public SignalRGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string session) : base(config, localStorage, log, http)
        {
            _session = session;
        }

        private void ReceiveStartClock(string message)
        {
            _log.LogInformation("-> SignalRGuest.ReceiveStartClock");

            _log.LogDebug($"message: {message}");

            CurrentSession.ClockMessage = JsonConvert.DeserializeObject<StartClockMessage>(message);

            _log.LogDebug($"CountDown: {CurrentSession.ClockMessage.CountDown}");
            _log.LogDebug($"Red: {CurrentSession.ClockMessage.Red}");
            _log.LogDebug($"ServerTime: {CurrentSession.ClockMessage.ServerTime}");
            _log.LogDebug($"Yellow: {CurrentSession.ClockMessage.Yellow}");

            RunClock();
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
    }
}