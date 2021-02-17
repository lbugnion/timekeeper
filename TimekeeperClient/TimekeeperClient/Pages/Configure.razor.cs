using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TimeKeeperApi.DataModel;
using TimekeeperClient.Model;

namespace TimekeeperClient.Pages
{
    public partial class Configure
    {
        public Session CurrentSession
        {
            get;
            private set;
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");

            var json = await LocalStorage.GetItemAsStringAsync(
                Constants.SessionStorageKey);

            Log.LogDebug($"json: {json}");

            if (!string.IsNullOrEmpty(json))
            {
                CurrentSession = JsonConvert.DeserializeObject<Session>(json);
                Log.LogDebug($"HIGHLIGHT--Found CurrentSession.SessionId: {CurrentSession.SessionId}");
            }
            else
            {
                // TODO Notify the user
            }

            Log.LogInformation("OnInitializedAsync ->");
        }
    }
}
