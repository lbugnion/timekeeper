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
        public SignalRGuest(
            IConfiguration config, 
            ILogger log, 
            HttpClient http) : base(config, log, http)
        {
        }

        private async Task ReceiveStartClock(string message)
        {
            _log.LogInformation("HIGHLIGHT---> SignalRGuest.ReceiveStartClock");

            _clockSettings = JsonConvert.DeserializeObject<StartClockMessage>(message);

            _log.LogDebug($"BlinkIfOver: {_clockSettings.BlinkIfOver}");
            _log.LogDebug($"CountDown: {_clockSettings.CountDown}");
            _log.LogDebug($"Red: {_clockSettings.Red}");
            _log.LogDebug($"ServerTime: {_clockSettings.ServerTime}");
            _log.LogDebug($"Yellow: {_clockSettings.Yellow}");

            await RunClock();
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRGuest.ConnectToServer");

            IsBusy = true;

            await CreateConnection();

            _connection.On<string>(Constants.StartClockMessageName, ReceiveStartClock);
            _connection.On<HostToGuestMessage>(Constants.HostToGuestMessageName, DisplayMessage);
            _connection.On(Constants.StopClockMessage, StopClock);

            await _connection.StartAsync();

            IsConnected = true;
            IsBusy = false;

            _log.LogInformation("SignalRGuest.ConnectToServer ->");
        }
    }
}
