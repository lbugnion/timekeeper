using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Timekeeper.Client.Model
{
    public class GuestSession : SessionBase
    {
        public const string SessionStorageKey = "GuestSessionStorageKey";

        public string UserName
        {
            get;
            set;
        }

        public GuestSession()
        {
            UserName = "Anonymous";
            Clocks = new List<Clock>();
        }

        public static async Task DeleteFromStorage(ILogger log = null)
        {
            await DeleteFromStorage(SessionStorageKey, log);
        }

        public static async Task<GuestSession> GetFromStorage(ILogger log)
        {
            var session = await GetFromStorage<GuestSession>(SessionStorageKey, log);
            return session;
        }

        protected override string GetSessionKey()
        {
            return SessionStorageKey;
        }
    }
}