using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Timekeeper.Client.Model
{
    public abstract class SessionBase
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

        [Required]
        public string UserId
        {
            get;
            set;
        }

        protected abstract string GetSessionKey();

        public SessionBase()
        {
            SessionId = Guid.NewGuid().ToString();
            SessionName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UserId = Guid.NewGuid().ToString();
        }

        protected static async Task DeleteFromStorage(string storageKey, ILogger log = null)
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

        public async Task Save(ILogger log)
        {
            log.LogTrace("CRITICAL--SAVING SESSION");

            var json = JsonConvert.SerializeObject(this);

            await _localStorage.SetItemAsync(
                GetSessionKey(),
                json);
        }

        public IList<Clock> Clocks
        {
            get;
            set;
        }
    }
}