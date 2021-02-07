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

            _log.LogInformation("-> SignalRHost.StartClock");

            IsStartDisabled = true;
            IsStopDisabled = false;

            _clockSettings = new StartClockMessage
            {
                BlinkIfOver = true, // TODO Make configurable
                CountDown = TimeSpan.FromSeconds(60), // TODO Make configurable
                Red = TimeSpan.FromSeconds(30), // TODO Make configurable
                Yellow = TimeSpan.FromSeconds(45), // TODO Make configurable
                ServerTime = DateTime.Now
            };

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_clockSettings));

                var functionKey = _config.GetValue<string>(StartClockKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var startClockUrl = $"{_hostName}/start";
                _log.LogDebug($"startClockUrl: {startClockUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, startClockUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

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

            try
            {
                CurrentMessage = InputMessage;

                var content = new StringContent(InputMessage);

                var functionKey = _config.GetValue<string>(SendMessageKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var sendMessageUrl = $"{_hostName}/send";
                _log.LogDebug($"startClockUrl: {sendMessageUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, sendMessageUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
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
            _log.LogInformation("StopAllClocks ->");
        }
    }
}
