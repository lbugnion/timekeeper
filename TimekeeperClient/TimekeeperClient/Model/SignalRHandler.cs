using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace TimekeeperClient.Model
{
    public abstract class SignalRHandler
    {
        protected const string HostNameKey = "HostName";

        public event EventHandler UpdateUi;

        protected bool _clockIsRunning;
        protected IConfiguration _config;
        protected HubConnection _connection;
        protected TimeSpan _countDown;
        protected HttpClient _http;
        protected ILogger _log;

        protected DateTime _startDateTime;
        protected string _hostName;
        private string clockDisplay;

        public string ClockDisplay
        {
            get => clockDisplay;
            protected set
            {
                clockDisplay = value;
                UpdateUi?.Invoke(this, EventArgs.Empty);
            }
        }

        public string CurrentMessage
        {
            get;
            protected set;
        }

        public bool IsBusy
        {
            get;
            protected set;
        }

        public bool IsConnected
        {
            get;
            protected set;
        }

        public SignalRHandler(
            IConfiguration config,
            ILogger log,
            HttpClient http)
        {
            ClockDisplay = "00:00:00";
            CurrentMessage = "Welcome!";

            _config = config;
            _log = log;
            _http = http;
        }

        protected void DisplayMessage(HostToGuestMessage message)
        {
            CurrentMessage = message.Message;
        }

        protected async Task CreateConnection()
        {
            _hostName = _config.GetValue<string>(HostNameKey);
            _log.LogDebug($"hostName: {_hostName}");

            var negotiateJson = await _http.GetStringAsync($"{_hostName}/api/negotiate");
            var negotiateInfo = JsonConvert.DeserializeObject<NegotiateInfo>(negotiateJson);

            _log.LogDebug($"HubName: {negotiateInfo.HubName}");

            _connection = new HubConnectionBuilder()
                .WithUrl(negotiateInfo.Url, options =>
                {
                    options.AccessTokenProvider = async () => negotiateInfo.AccessToken;
                })
                .Build();
        }

        protected void StopClock()
        {
            _log.LogInformation("HIGHLIGHT---> StopClock");
            _clockIsRunning = false;
        }

        public abstract Task Connect();
    }
}