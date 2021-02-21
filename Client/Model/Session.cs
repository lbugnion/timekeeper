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

        [Required]
        public string UserId
        {
            get;
            set;
        }

        public IList<Clock> Clocks
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

        public static async Task<Session> GetFromStorage(ILogger log)
        {
            var json = await _localStorage.GetItemAsStringAsync(
                SessionStorageKey);

            log.LogDebug($"json: {json}");

            if (!string.IsNullOrEmpty(json))
            {
                var session = JsonConvert.DeserializeObject<Session>(json);

                // Reset the UI objects
                // TODO it would be cleaner to NOT save the Clock object
                // and to recreate them when the session is read from storage
                foreach (var clock in session.Clocks)
                {
                    clock.ResetDisplay();
                    clock.CurrentBackgroundColor = Clock.DefaultBackgroundColor;
                    clock.IsClockRunning = false;
                    clock.IsConfigDisabled = false;
                    clock.IsDeleteDisabled = false;
                    clock.IsStartDisabled = false;
                    clock.IsStopDisabled = false;
                }

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
