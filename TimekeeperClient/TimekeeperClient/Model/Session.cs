using Blazored.LocalStorage;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace TimekeeperClient.Model
{
    public class Session
    {
        [Required]
        public string SessionName
        {
            get;
            set;
        }

        [Required]
        public string SessionId
        {
            get;
            set;
        }

        public StartClockMessage ClockMessage
        {
            get;
            set;
        }

        public const string SessionStorageKey = "SessionStorageKey";
        private static ILocalStorageService _localStorage;

        public async Task Save()
        {
            var json = JsonConvert.SerializeObject(this);

            await _localStorage.SetItemAsync(
                SessionStorageKey,
                json);
        }

        public static void SetLocalStorage(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public static async Task<Session> GetFromStorage()
        {
            var json = await _localStorage.GetItemAsStringAsync(
                SessionStorageKey);

            if (!string.IsNullOrEmpty(json))
            {
                var session = JsonConvert.DeserializeObject<Session>(json);
                return session;
            }

            return null;
        }

        public async Task DeleteFromStorage()
        {
            await _localStorage.RemoveItemAsync(SessionStorageKey);
        }
    }
}
