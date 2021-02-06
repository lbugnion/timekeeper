using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace TimekeeperClient.Model
{
    public class SignalRHost : SignalRHandler
    {
        public SignalRHost(
            IConfiguration config, 
            ILogger log, 
            HttpClient http) : base(config, log, http)
        {
        }

        public async Task StartClock()
        {
            if (_clockIsRunning)
            {
                return;
            }

            _log.LogInformation("HIGHLIGHT---> SignalRHost.StartClock");

            _clockSettings = new StartClockMessage
            {
                BlinkIfOver = true, // TODO Make configurable
                CountDown = TimeSpan.FromSeconds(90), // TODO Make configurable
                Red = TimeSpan.FromSeconds(30), // TODO Make configurable
                Yellow = TimeSpan.FromMinutes(1), // TODO Make configurable
                ServerTime = DateTime.Now
            };

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_clockSettings));
                var startClockUrl = $"{_hostName}/api/start";
                var response = await _http.PostAsync(startClockUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    await RunClock();
                }
                else
                {
                    CurrentMessage = "Unable to communicate with clients";
                    // TODO Show a warning message
                }
            }
            catch
            {
                CurrentMessage = "Unable to communicate with clients";
                // TODO Show a warning message
            }
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> SignalRHost.ConnectToServer");

            IsBusy = true;

            await CreateConnection();
            await _connection.StartAsync();

            IsConnected = true;
            IsBusy = false;

            _log.LogInformation("SignalRHost.ConnectToServer ->");
        }

        public string InputMessage
        {
            get;
            set;
        }

        public async Task SendMessage()
        {
            _log.LogInformation("-> SendMessage");

            if (string.IsNullOrEmpty(InputMessage))
            {
                return;
            }

            var content = new StringContent(InputMessage);

            var sendMessageUrl = $"{_hostName}/api/send";
            await _http.PostAsync(sendMessageUrl, content);
        }

        public async Task StopAllClocks()
        {
            StopClock(null);

            // Notify clients
            var stopClockUrl = $"{_hostName}/api/stop";
            await _http.GetAsync(stopClockUrl);
        }
    }
}
