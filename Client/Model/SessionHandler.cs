using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class SessionHandler
    {
        private static ILocalStorageService _localStorage;

        public async Task DeleteFromStorage(string storageKey, ILogger log = null)
        {
            log?.LogDebug($"Deleting session {storageKey} from storage");
            await _localStorage.RemoveItemAsync(storageKey);
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

        public SessionHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SaveToStorage(SessionBase session, string sessionStorageKey, ILogger log)
        {
            log.LogTrace("SAVING SESSION");

            var json = JsonConvert.SerializeObject(session);

            await _localStorage.SetItemAsync(
                sessionStorageKey,
                json);
        }
    }
}
