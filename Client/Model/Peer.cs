using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class Peer
    {
        private static ILocalStorageService _localStorage;
        private static ILogger _log;

        public PeerMessage Message
        {
            get;
            internal set;
        }

        public Peer(string peerId)
        {
            Message = new PeerMessage
            {
                PeerId = peerId
            };
        }

        public static async Task<PeerMessage> GetFromStorage(string key)
        {
            var json = await _localStorage.GetItemAsStringAsync(key);

            _log.LogDebug($"Getting: {json}");

            if (!string.IsNullOrEmpty(json))
            {
                var savedGuest = JsonConvert.DeserializeObject<PeerMessage>(json);
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

        public async Task DeleteFromStorage(string key)
        {
            await _localStorage.RemoveItemAsync(key);
        }

        public async Task Save(string key)
        {
            var json = JsonConvert.SerializeObject(Message, Formatting.Indented);

            _log.LogDebug($"Saving: {json}");

            await _localStorage.SetItemAsync(
                key,
                json);
        }
    }
}