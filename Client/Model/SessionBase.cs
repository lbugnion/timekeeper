using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class SessionBase
    {
        protected static ILocalStorageService _localStorage;

        [Required]
        public string SessionId
        {
            get;
            set;
        }

        [Required]
        public string SessionName
        {
            get;
            set;
        }

        public string LastMessage
        {
            get;
            set;
        }

        [Required]
        public string UserId
        {
            get;
            set;
        }

        public SessionBase()
        {
            SessionId = Guid.NewGuid().ToString();
            SessionName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UserId = Guid.NewGuid().ToString();
            UserName = GuestMessage.AnonymousName;
            Clocks = new List<Clock>();
        }

        public static async Task DeleteFromStorage(string storageKey, ILogger log = null)
        {
            log?.LogDebug($"Deleting session {storageKey} from storage");
            await _localStorage.RemoveItemAsync(storageKey);
        }

        public static async Task<T> GetFromStorage<T>(string storageKey, ILogger log)
            where T : SessionBase
        {
            log.LogInformation("-> GetFromStorage");
            log.LogDebug($"storageKey: {storageKey}");

            var json = await _localStorage.GetItemAsStringAsync(
                storageKey);

            log.LogDebug($"json: {json}");

            if (!string.IsNullOrEmpty(json))
            {
                var session = JsonConvert.DeserializeObject<T>(json);
                return session;
            }

            return null;
        }

        public static void SetLocalStorage(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task Save(string sessionStorageKey, ILogger log)
        {
            log.LogTrace("SAVING SESSION");

            var json = JsonConvert.SerializeObject(this);

            await _localStorage.SetItemAsync(
                sessionStorageKey,
                json);
        }

        public IList<Clock> Clocks
        {
            get;
            set;
        }

        public static async Task<SessionBase> GetFromStorage(string sessionStorageKey, ILogger log)
        {
            var session = await GetFromStorage<SessionBase>(sessionStorageKey, log);

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
                }
            }

            return session;
        }

        public bool CreatedFromTemplate
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }
    }
}