using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timekeeper.Client.Model
{
    public class HostSession : SessionBase
    {
        public const string SessionStorageKey = "SessionStorageKey";

        public bool CreatedFromTemplate
        {
            get;
            set;
        }

        public HostSession()
        {
            Clocks = new List<Clock>();
        }

        public static async Task DeleteFromStorage(ILogger log = null)
        {
            await DeleteFromStorage(SessionStorageKey, log);
        }

        public static async Task<HostSession> GetFromStorage(ILogger log)
        {
            var session = await GetFromStorage<HostSession>(SessionStorageKey, log);

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
                    clock.IsStartDisabled = false;
                    clock.IsStopDisabled = false;
                }
            }

            return session;
        }

        protected override string GetSessionKey()
        {
            return SessionStorageKey;
        }
    }
}