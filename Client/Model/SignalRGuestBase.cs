using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Timekeeper.Client.Model
{
    public abstract class SignalRGuestBase : SignalRHandler
    {
        protected string _sessionId;
        protected string _unregisterFromGroup = null;
     
        protected override string PeerKey => "GuestPeer";

        public SignalRGuestBase(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string sessionId,
            SessionHandler session) : base(config, localStorage, log, http, session)
        {
            _log.LogInformation("> SignalRGuestBase()");
            _sessionId = sessionId;
        }
    }
}
