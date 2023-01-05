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
        private readonly IConfiguration _config;
        private readonly string _hostName;
        private readonly string _hostNameFree;
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private string _token;

        public IList<SessionBase> CloudSessions
        {
            get;
            private set;
        }

        public string ErrorStatus
        {
            get;
            private set;
        }

        public SessionBase NewSession
        {
            get;
            private set;
        }

        public EditContext NewSessionEditContext
        {
            get;
            private set;
        }

        public int State
        {
            get;
            internal set;
        }

        public string Status
        {
            get;
            private set;
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
            _hostNameFree = _config.GetValue<string>(Constants.HostNameFreeKey);
        }

        public async Task<string> CheckSetNewSession(ILogger log)
        {
            var isValid = NewSessionEditContext.Validate();
            if (isValid)
            {
                NewSession.Clocks.Add(new Clock());
                await Save(NewSession, SignalRHost.HostSessionKey, log);
                return NewSession.SessionId;
            }

            return null;
        }

        public async Task DeleteFromStorage(
            string storageKey,
            ILogger log = null)
        {
            log?.LogDebug($"Deleting session {storageKey} from storage");
            await _localStorage.RemoveItemAsync(storageKey);
        }

        public async Task DeleteSession(string sessionId, ILogger log)
        {
            log.LogInformation("-> DeleteSession");

            var selectedSession = CloudSessions.FirstOrDefault(s => s.SessionId == sessionId);

            if (selectedSession == null)
            {
                throw new ArgumentException($"Invalid sessionId {sessionId}");
            }

            foreach (var clock in selectedSession.Clocks)
            {
                clock.IsClockRunning = false;
                clock.IsConfigDisabled = true;
                clock.IsNudgeDisabled = true;
                clock.IsPlayStopDisabled = true;
                clock.IsSelected = false;
                clock.ResetDisplay();
            }

            // TODO Send notify-delete message
            // TODO Send delete message

            await DeleteFromStorage(SignalRHost.HostSessionKey, log);
            State = 2;
        }

        public async Task<string> Duplicate(string sessionId, ILogger log)
        {
            log.LogInformation("-> Duplicate");

            var modelSession = CloudSessions
                .FirstOrDefault(s => s.SessionId == sessionId);

            if (modelSession == null)
            {
                throw new ArgumentException($"Invalid sessionId {sessionId}");
            }

            var newSession = new SessionBase
            {
                BranchId = modelSession.BranchId,
                Clocks = new List<Clock>(),
                SessionId = Guid.NewGuid().ToString(),
                SessionName = $"{modelSession.SessionName} (copy)"
            };

            foreach (var clock in modelSession.Clocks)
            {
                var duplicatedClock = new Clock();
                duplicatedClock.Update(clock.Message, false);
                duplicatedClock.CurrentLabel = clock.Message.Label;
                newSession.Clocks.Add(duplicatedClock);
            }

            var success = await Save(newSession, SignalRHost.HostSessionKey, log);
            if (success)
            {
                CloudSessions.Add(newSession);
                return newSession.SessionId;
            }

            return null;
        }

        public async Task<SessionBase> GetFromStorage(
                    string storageKey,
            ILogger log)
        {
            log.LogInformation("-> GetFromStorage");
            log.LogDebug($"storageKey: {storageKey}");

            var json = await _localStorage.GetItemAsStringAsync(
                storageKey);

            //log.LogDebug($"json: {json}");

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
                    clock.CurrentForegroundColor = Clock.DefaultForegroundColor;
                    clock.CurrentLabel = clock.Message.Label;
                    clock.IsClockRunning = false;
                    clock.IsConfigDisabled = false;
                    clock.IsPlayStopDisabled = false;
                }
            }

            return session;
        }

        public async Task<IList<SessionBase>> GetSessions(
            ILogger log)
        {
            log.LogInformation("-> SessionHandler.Get");

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

                log.LogDebug(json);

                var sessions = JsonConvert.DeserializeObject<IEnumerable<SessionBase>>(json);
                CloudSessions = sessions.Where(s => s != null).ToList();
                return CloudSessions;
            }
            catch (Exception ex)
            {
                ErrorStatus = "Unable to load sessions";
                throw ex;
            }
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

        public async Task<bool> Save(
            SessionBase session,
            string sessionStorageKey,
            ILogger log)
        {
            var json = JsonConvert.SerializeObject(session);
            var content = new StringContent(json);

            if (string.IsNullOrEmpty(_token))
            {
                var tokenUrl = $"{_hostName}/token";

                try
                {
                    _token = await _http.GetStringAsync(tokenUrl);

                    if (string.IsNullOrEmpty(_token))
                    {
                        log.LogError($"Cannot save session: cannot find token");
                        ErrorStatus = "Error saving, please contact support@timekeeper.cloud";
                        return false;
                    }
                }
                catch
                {
                    log.LogError($"Cannot save session: cannot find token");
                    ErrorStatus = "Error saving, please contact support@timekeeper.cloud";
                    return false;
                }
            }

            var saveSessionUrl = $"{_hostNameFree}/session/{session.BranchId}/{session.SessionId}";
            log.LogDebug($"saveSessionUrl: {saveSessionUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, saveSessionUrl)
            {
                Content = content,
            };

            httpRequest.Headers.Add(Constants.TokenHeaderKey, _token);

            try
            {
                var response = await _http.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    log.LogDebug($"Saved session {session.SessionId} / {session.SessionName} to the cloud");
                    Status = "Session saved to the cloud";

                    await SaveToStorage(session, sessionStorageKey, log);

                    return true;
                }
                else
                {
                    log.LogError($"Cannot save session: {response.ReasonPhrase}");
                    ErrorStatus = "Error saving, please reload the page";
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Cannot save session: {ex.Message}");
                ErrorStatus = "Error saving the session to the cloud";
                return false;
            }
        }

        public async Task SaveToStorage(
            SessionBase session,
            string sessionStorageKey,
            ILogger log)
        {
            log.LogInformation("-> SessionHandler.SaveToStorage");

            var json = JsonConvert.SerializeObject(session, Formatting.Indented);

            await _localStorage.SetItemAsync(
                sessionStorageKey,
                json);
        }

        public async Task SelectSession(string sessionId, ILogger log)
        {
            log.LogInformation("-> SelectSession");

            var selectedSession = CloudSessions.FirstOrDefault(s => s.SessionId == sessionId);

            if (selectedSession == null)
            {
                throw new ArgumentException($"Invalid sessionId {sessionId}");
            }

            foreach (var clock in selectedSession.Clocks)
            {
                clock.IsClockRunning = false;
                clock.IsConfigDisabled = true;
                clock.IsNudgeDisabled = true;
                clock.IsPlayStopDisabled = true;
                clock.IsSelected = false;
                clock.ResetDisplay();
            }

            await SaveToStorage(selectedSession, SignalRHostBase.HostSessionKey, log);
            State = 2;
        }
    }
}