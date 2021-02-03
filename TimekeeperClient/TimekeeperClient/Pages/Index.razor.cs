using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using TimeKeeperApi;

namespace TimekeeperClient.Pages
{
    public partial class Index
    {
        private const string HostNameKey = "HostName";

        private HubConnection _connection;

        public bool IsBusy
        {
            get;
            private set;
        }

        public bool IsConnected
        {
            get;
            private set;
        }

        protected override async Task OnInitializedAsync()
        {
            await ConnectToServerAsync();
        }

        private async Task ConnectToServerAsync()
        {
            Log.LogInformation("-> ConnectToServerAsync");

            IsBusy = true;

            var hostName = Config.GetValue<string>(HostNameKey);

            Log.LogDebug($"hostName: {hostName}");

            var negotiateJson = await Http.GetStringAsync($"{hostName}/api/negotiate");
            var negotiateInfo = JsonConvert.DeserializeObject<NegotiateInfo>(negotiateJson);

            Log.LogDebug($"HubName: {negotiateInfo.HubName}");

            _connection = new HubConnectionBuilder()
                .WithUrl(negotiateInfo.Url, options =>
                {
                    options.AccessTokenProvider = async () => negotiateInfo.AccessToken;
                })
                .Build();

            _connection.On<string>(Constants.StartClockMessageName, ReceiveMessage);

            await _connection.StartAsync();

            IsConnected = true;
            IsBusy = false;

            //await _hubConnection.SendAsync(, _username, message);

            Log.LogInformation("ConnectToServerAsync ->");
        }

        private void ReceiveMessage(string message)
        {
            Log.LogInformation("HIGHLIGHT---> ReceiveMessage");
            Log.LogDebug($"message: {message}");
        }
    }
}
