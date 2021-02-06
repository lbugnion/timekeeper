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

        public SignalRHost(
            IConfiguration config, 
            ILogger log, 
            HttpClient http) : base(config, log, http)
        {
            IsStopDisabled = true;
        }

        protected override void DisplayMessage(string message)
        {
            base.DisplayMessage(message);
            Status = "Message sent";
        }

        public async Task StartClock()
        {
            if (_clockIsRunning)
            {
                return;
            }

            _log.LogInformation("HIGHLIGHT---> SignalRHost.StartClock");

            IsStartDisabled = true;
            IsStopDisabled = false;

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
                    ErrorStatus = "Unable to communicate with clients";
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

            var ok = (await CreateConnection())
                && (await StartConnection());

            if (ok)
            {
                IsConnected = true;
                IsInError = false;
            }
            else
            {
                IsInError = true;
                IsConnected = false;
            }

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

            try
            {
                var stopClockUrl = $"{_hostName}/api/stop";
                await _http.GetAsync(stopClockUrl);
            }
            catch (Exception ex)
            {
                _log.LogError($"Error sending stop instruction: {ex.Message}");
                ErrorStatus = "Couldn't reach the guests";
            }

            IsStartDisabled = false;
            IsStopDisabled = true;
        }
    }
}
