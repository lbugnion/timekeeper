using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class Guest
    {
        public const string GuestStorageKey = "GuestStorageKey";
        private static ILocalStorageService _localStorage;
        private static ILogger _log;

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

        public async Task Save()
        {
            var json = JsonConvert.SerializeObject(this);

            _log.LogDebug($"HIGHLIGHT--Saving: {json}");

            await _localStorage.SetItemAsync(
                GuestStorageKey,
                json);
        }

        public static void SetLocalStorage(
            ILocalStorageService localStorage, 
            ILogger log)
        {
            _localStorage = localStorage;
            _log = log;
        }

        public static async Task<Guest> GetFromStorage()
        {
            var json = await _localStorage.GetItemAsStringAsync(
                GuestStorageKey);

            _log.LogDebug($"Getting: {json}");

            if (!string.IsNullOrEmpty(json))
            {
                var guest = JsonConvert.DeserializeObject<Guest>(json);

                _log.LogDebug($"Guest name: {guest.Message.DisplayName}");

                return guest;
            }

            return null;
        }

        public async Task DeleteFromStorage()
        {
            await _localStorage.RemoveItemAsync(GuestStorageKey);
        }
    }
}
