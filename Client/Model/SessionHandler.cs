using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class SessionHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _hostName;

        public async Task DeleteFromStorage(
            string storageKey,
            ILogger log = null)
        {
            log?.LogDebug($"Deleting session {storageKey} from storage");
            await _localStorage.RemoveItemAsync(storageKey);
        }

        public async Task<IList<SessionBase>> GetSessions(
            string storageKey,
            ILogger log)
        {
            log.LogInformation("-> SessionHandler.Get");
            log.LogDebug($"storageKey: {storageKey}");

            var getSessionsUrl = $"{_hostName}/sessions";
            log.LogDebug($"getSessionsUrl: {getSessionsUrl}");

            try
            {
                var json = await _http.GetStringAsync(getSessionsUrl);

                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                var session = JsonConvert.DeserializeObject<IList<SessionBase>>(json);
                return session;
            }
            catch (Exception ex)
            {
                log.LogError($"Cannot get session: {ex.Message}");
                return null;
            }
        }

        public async Task<SessionBase> GetFromStorage(
            string storageKey, 
            ILogger log)
        {
            log.LogInformation("-> GetFromStorage");
            log.LogDebug($"storageKey: {storageKey}");

            var json = await _localStorage.GetItemAsStringAsync(
                storageKey);

            log.LogDebug($"json: {json}");

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var session = JsonConvert.DeserializeObject<SessionBase>(json);

            if (session != null
                && session.Clocks != null)
            {
                // Reset the UI objects
                foreach (var clock in session.Clocks)
                {
                    clock.ResetDisplay();
                    clock.CurrentBackgroundColor = Clock.DefaultBackgroundColor;
                    clock.IsClockRunning = false;
                    clock.IsConfigDisabled = false;
                    clock.IsDeleteDisabled = false;
                    clock.IsPlayStopDisabled = false;
                }
            }

            return session;
        }

        public SessionHandler(
            ILocalStorageService localStorage,
            HttpClient http,
            IConfiguration config)
        {
            _localStorage = localStorage;
            _http = http;
            _config = config;

            _hostName = _config.GetValue<string>(Constants.HostNameKey);
        }

        public async Task SaveToStorage(
            SessionBase session, 
            string sessionStorageKey, 
            ILogger log)
        {
            log.LogInformation("-> SessionHandler.SaveToStorage");

            var json = JsonConvert.SerializeObject(session);

            await _localStorage.SetItemAsync(
                sessionStorageKey,
                json);
        }

        public SessionBase CurrentSession
        {
            get;
            protected set;
        }

        public string ErrorStatus
        {
            get;
            private set;
        }

        public string Status
        {
            get;
            private set;
        }

        public async Task LoadAllSessions(ILogger log)
        {
            throw new NotImplementedException();
        }
    }
}
