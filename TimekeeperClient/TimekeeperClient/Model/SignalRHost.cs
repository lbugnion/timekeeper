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
        private StartClockMessage _clockSettings;

        public bool IsYellow
        {
            get;
            private set;
        }

        public bool IsRed
        {
            get;
            private set;
        }

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

            _startDateTime = DateTime.Now;
            _countDown = TimeSpan.FromSeconds(90); // TODO Make configurable

            _clockSettings = new StartClockMessage
            {
                BlinkIfOver = true, // TODO Make configurable
                CountDown = _countDown,
                Red = TimeSpan.FromSeconds(30), // TODO Make configurable
                Yellow = TimeSpan.FromMinutes(1), // TODO Make configurable
                ServerTime = _startDateTime
            };

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_clockSettings));
                var startClockUrl = $"{_hostName}/api/start";
                await _http.PostAsync(startClockUrl, content);
            }
            catch
            {
            }

            _clockIsRunning = true;

            await Task.Run(async () =>
            {
                _log.LogDebug($"Red: {_clockSettings.Red.TotalSeconds} seconds");
                _log.LogDebug($"Yellow: {_clockSettings.Yellow.TotalSeconds} seconds");

                do
                {
                    if (_clockIsRunning)
                    {
                        var elapsed = DateTime.Now - _startDateTime;
                        var remains = _countDown - elapsed;
                        ClockDisplay = remains.ToString(@"hh\:mm\:ss");

                        if (remains.TotalSeconds <   _clockSettings.Red.TotalSeconds)
                        {
                            _log.LogTrace("HIGHLIGHT--Red");
                            IsRed = true;
                        }

                        if (remains.TotalSeconds < _clockSettings.Yellow.TotalSeconds)
                        {
                            _log.LogTrace("HIGHLIGHT--Yellow");
                            IsYellow = true;
                        }

                    }

                    await Task.Delay(1000);
                }
                while (_clockIsRunning);
            });
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

        public async Task StopAllClocks()
        {
            StopClock();

            var stopClockUrl = $"{_hostName}/api/stop";
            await _http.GetAsync(stopClockUrl);
        }
    }
}
