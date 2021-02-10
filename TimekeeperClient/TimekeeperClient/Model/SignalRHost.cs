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

        public SignalRHost(
            IConfiguration config,
            ILogger log,
            HttpClient http) : base(config, log, http)
        {
            IsStopDisabled = true;
            base.CountdownFinished += SignalRHostCountdownFinished;
        }

        private void SignalRHostCountdownFinished(object sender, EventArgs e)
        {
            _log.LogInformation("-> SignalRHostCountdownFinished");
            IsStartDisabled = false;
            IsStopDisabled = true;
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

            var ok = (await CreateConnection())
                && (await StartConnection());

            if (ok)
            {
                IsConnected = true;
                IsInError = false;
                IsStartDisabled = false;
                IsStopDisabled = true;
                IsSendMessageDisabled = false;
                CurrentMessage = "Ready";
            }
            else
            {
                IsInError = true;
                IsConnected = false;
                IsStartDisabled = true;
                IsStopDisabled = true;
                IsSendMessageDisabled = true;
                CurrentMessage = "Error";
            }

            IsBusy = false;
            _log.LogInformation("SignalRHost.ConnectToServer ->");
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

                _log.LogDebug($"HIGHLIGHT--GroupName: {Program.GroupInfo.GroupId}");

                var content = new StringContent(InputMessage);

                var functionKey = _config.GetValue<string>(SendMessageKeyKey);
                _log.LogDebug($"functionKey: {functionKey}");

                var sendMessageUrl = $"{_hostName}/send";
                _log.LogDebug($"startClockUrl: {sendMessageUrl}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, sendMessageUrl);
                httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, Program.GroupInfo.GroupId);
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

            _clockSettings = new StartClockMessage
            {
                CountDown = TimeSpan.FromMinutes(5), // TODO Make configurable
                Red = TimeSpan.FromSeconds(30), // TODO Make configurable
                Yellow = TimeSpan.FromSeconds(120), // TODO Make configurable
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
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, Program.GroupInfo.GroupId);
                httpRequest.Content = content;

                var response = await _http.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    RunClock();
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
                httpRequest.Headers.Add(Constants.GroupIdHeaderKey, Program.GroupInfo.GroupId);
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