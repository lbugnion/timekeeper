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
        protected HttpClient _http;
        protected ILogger _log;
        protected string _hostName;
        private string _clockDisplay;

        public string Status
        {
            get => _status;
            protected set
            {
                _status = value;

                if (!string.IsNullOrEmpty(_status))
                {
                    ErrorStatus = string.Empty;
                }
            }
        }

        public string ErrorStatus
        {
            get => _errorStatus;
            protected set
            {
                _errorStatus = value;

                if (!string.IsNullOrEmpty(_errorStatus))
                {
                    Status = string.Empty;
                }
            }
        }

        public bool IsYellow
        {
            get;
            protected set;
        }

        public bool IsRed
        {
            get;
            protected set;
        }

        protected StartClockMessage _clockSettings;
        private string _status;
        private string _errorStatus;

        protected async Task RunClock()
        {
            _clockIsRunning = true;

            Task.Run(async () =>
            {
                do
                {
                    if (_clockIsRunning)
                    {
                        var elapsed = DateTime.Now - _clockSettings.ServerTime;
                        var remains = _clockSettings.CountDown - elapsed;
                        ClockDisplay = remains.ToString(@"hh\:mm\:ss");

                        if (remains.TotalSeconds < _clockSettings.Red.TotalSeconds)
                        {
                            IsRed = true;
                        }

                        if (remains.TotalSeconds < _clockSettings.Yellow.TotalSeconds)
                        {
                            IsYellow = true;
                        }
                    }

                    await Task.Delay(1000);
                }
                while (_clockIsRunning);
            });
        }

        public string ClockDisplay
        {
            get => _clockDisplay;
            protected set
            {
                _clockDisplay = value;
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

        public bool IsInError
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
            Status = "Please wait...";

            _config = config;
            _log = log;
            _http = http;
        }

        protected virtual void DisplayMessage(string message)
        {
            _log.LogInformation("-> DisplayMessage");
            _log.LogDebug(message);

            CurrentMessage = message;
            UpdateUi?.Invoke(this, EventArgs.Empty);
        }

        protected async Task<bool> CreateConnection()
        {
            _log.LogInformation("-> SignalRHandler.CreateConnection");

            _hostName = _config.GetValue<string>(HostNameKey);
            _log.LogDebug($"hostName: {_hostName}");

            NegotiateInfo negotiateInfo = null;

            try
            {
                var negotiateJson = await _http.GetStringAsync($"{_hostName}/api/negotiate");
                negotiateInfo = JsonConvert.DeserializeObject<NegotiateInfo>(negotiateJson);
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the function: {ex.Message}");
                ErrorStatus = "Error with the backend, please contact support";
                _log.LogInformation("SignalRHandler.CreateConnection ->");
                return false;
            }

            _log.LogDebug($"HubName: {negotiateInfo.HubName}");

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(negotiateInfo.Url, options =>
                    {
                        options.AccessTokenProvider = async () => negotiateInfo.AccessToken;
                    })
                    .Build();
            }
            catch (Exception ex)
            {
                _log.LogError($"Error reaching the SignalR service: {ex.Message}");
                ErrorStatus = "Error connecting, please contact support";
                return false;
            }

            Status = "Ready...";
            _log.LogInformation("SignalRHandler.CreateConnection ->");
            return true;
        }

        protected async Task<bool> StartConnection()
        {
            _log.LogInformation("-> SignalRHandler.StartConnection");

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                _log.LogError($"Error starting the connection: {ex.Message}");
                ErrorStatus = "Error connecting, please contact support";
                return false;
            }

            Status = "Connected!";
            
            _log.LogInformation("SignalRHandler.StartConnection ->");
            return true;
        }

        protected void StopClock(object _)
        {
            _log.LogInformation("HIGHLIGHT---> StopClock");
            _clockIsRunning = false;
            Status = "Clock was stopped";
        }

        public abstract Task Connect();
    }
}