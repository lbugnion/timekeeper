using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public EditContext NewSessionEditContext
        {
            get;
            private set;
        }

        public IList<SessionBase> CloudSessions
        {
            get;
            private set;
        }

        public SessionBase NewSession
        {
            get;
            private set;
        }

        public bool CheckSetNewSession()
        {
            var isValid = NewSessionEditContext.Validate();
            if (isValid)
            {
                CurrentSession = NewSession;
                return true;
            }

            return false;
        }

        public async Task DeleteFromStorage(
            string storageKey,
            ILogger log = null)
        {
            log?.LogDebug($"Deleting session {storageKey} from storage");
            await _localStorage.RemoveItemAsync(storageKey);
        }

        public async Task<IList<SessionBase>> GetSessions(
            ILogger log)
        {
            log.LogInformation("HIGHLIGHT---> SessionHandler.Get");

            var branchId = _config.GetValue<string>(Constants.BranchIdKey);
            log.LogDebug($"branchId: {branchId}");
            var getSessionsUrl = $"{_hostName}/sessions/{branchId}";
            log.LogDebug($"getSessionsUrl: {getSessionsUrl}");

            try
            {
                var json = await _http.GetStringAsync(getSessionsUrl);

                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                CloudSessions = JsonConvert.DeserializeObject<IList<SessionBase>>(json);
                return CloudSessions;
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

        public async Task SelectSession(string sessionId, ILogger log)
        {
            log.LogInformation("HIGHLIGHT---> SelectSession");

            var selectedSession = CloudSessions.FirstOrDefault(s => s.SessionId == sessionId);

            if (selectedSession == null)
            {
                throw new ArgumentException($"Invalid sessionId {sessionId}");
            }

            CurrentSession = selectedSession;
            await SaveToStorage(CurrentSession, SignalRHost.HostSessionKey, log);
            State = 2;
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

        public void InitializeContext(ILogger log)
        {
            log.LogInformation("-> SessionHandler.InitializeContext");

            NewSession = new SessionBase
            {
                BranchId = _config.GetValue<string>(Constants.BranchIdKey),
                SessionId = Guid.NewGuid().ToString(),
                SessionName = null
            };

            NewSessionEditContext = new EditContext(NewSession);
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
            private set;
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

        public int State 
        { 
            get; 
            internal set; 
        }
    }
}
