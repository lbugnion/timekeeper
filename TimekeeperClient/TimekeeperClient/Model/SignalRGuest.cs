using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using TimeKeeperApi.DataModel;

namespace TimekeeperClient.Model
{
    public class SignalRGuest : SignalRHandler
    {
        private TimeSpan _offset;

        public SignalRGuest(
            IConfiguration config, 
            ILogger log, 
            HttpClient http) : base(config, log, http)
        {
        }

        private async Task ReceiveStartClock(StartClockMessage message)
        {
            _log.LogInformation("HIGHLIGHT---> SignalRGuest.ReceiveStartClock");

            _startDateTime = message.ServerTime;
            _offset = DateTime.Now - _startDateTime;
            _countDown = message.CountDown;
            _clockIsRunning = true;

            await Task.Run(() =>
            {
                do
                {
                    if (_clockIsRunning)
                    {
                        var elapsed = DateTime.Now - _startDateTime + _offset;
                        ClockDisplay = (_countDown - elapsed).ToString("hh:mm:ss");
                    }
                }
                while (_clockIsRunning);
            });
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRGuest.ConnectToServer");

            IsBusy = true;

            await CreateConnection();

            _connection.On<StartClockMessage>(Constants.StartClockMessageName, ReceiveStartClock);
            _connection.On<HostToGuestMessage>(Constants.HostToGuestMessageName, DisplayMessage);
            _connection.On(Constants.StopClockMessage, StopClock);

            await _connection.StartAsync();

            IsConnected = true;
            IsBusy = false;

            _log.LogInformation("SignalRGuest.ConnectToServer ->");
        }
    }
}
