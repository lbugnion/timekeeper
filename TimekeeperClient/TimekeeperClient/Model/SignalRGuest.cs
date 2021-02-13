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
        public SignalRGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http) : base(config, localStorage, log, http)
        {
        }

        private void ReceiveStartClock(string message)
        {
            _log.LogInformation("-> SignalRGuest.ReceiveStartClock");

            _log.LogDebug($"message: {message}");

            _clockSettings = JsonConvert.DeserializeObject<StartClockMessage>(message);

            _log.LogDebug($"CountDown: {_clockSettings.CountDown}");
            _log.LogDebug($"Red: {_clockSettings.Red}");
            _log.LogDebug($"ServerTime: {_clockSettings.ServerTime}");
            _log.LogDebug($"Yellow: {_clockSettings.Yellow}");

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

            // TODO REMOVE DEBUG CODE
#if DEBUG
            var ok = await InitializeSession("d857f80b-aa3a-4abe-b76b-fbdb2fec47e8")
                && await CreateConnection();
#else
            var ok = await InitializeSession()
                && await CreateConnection();
#endif

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