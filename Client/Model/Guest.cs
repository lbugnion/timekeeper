using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class Guest
    {
        private static ILocalStorageService _localStorage;
        private static ILogger _log;
        public const string GuestStorageKey = "GuestStorageKey";

        public GuestMessage Message
        {
            get;
            internal set;
        }

        public Guest(string guestId)
        {
            Message = new GuestMessage
            {
                GuestId = guestId
            };
        }

        public static async Task<GuestMessage> GetFromStorage()
        {
            var json = await _localStorage.GetItemAsStringAsync(
                GuestStorageKey);

            _log.LogDebug($"Getting: {json}");

            if (!string.IsNullOrEmpty(json))
            {
                var savedGuest = JsonConvert.DeserializeObject<GuestMessage>(json);
                _log.LogDebug($"Guest name: {savedGuest.DisplayName}");

                return savedGuest;
            }

            return null;
        }

        public static void SetLocalStorage(
            ILocalStorageService localStorage,
            ILogger log)
        {
            _localStorage = localStorage;
            _log = log;
        }

        public async Task DeleteFromStorage()
        {
            await _localStorage.RemoveItemAsync(GuestStorageKey);
        }

        public async Task Save()
        {
            var json = JsonConvert.SerializeObject(Message);

            _log.LogDebug($"Saving: {json}");

            await _localStorage.SetItemAsync(
                GuestStorageKey,
                json);
        }
    }
}