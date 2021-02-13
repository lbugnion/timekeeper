using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace TimekeeperClient.Pages
{
    public partial class Index
    {
        public const string NormalBackgroundClassName = "background-normal";
        public const string RedBackgroundClassName = "background-red";
        public const string RunningBackgroundClassName = "background-running";
        public const string YellowBackgroundClassName = "background-yellow";

        public string ClientVersion
        {
            get;
            private set;
        }

        public string Environment
        {
            get;
            private set;
        }

        protected override void OnInitialized()
        {
            try
            {
                var version = Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version;

                Log.LogDebug($"Full version: {version}");
                ClientVersion = $"V{version.ToString(4)}";
                Log.LogDebug($"clientVersion: {ClientVersion}");

                var environment = Config.GetValue<string>("Environment");
                if (environment == "Production")
                {
                    environment = string.Empty;
                }

                Environment = environment;
            }
            catch
            {
                Log.LogWarning($"Assembly not found");
                ClientVersion = "N/A";
            }
        }

        public void LogInHost()
        {
            Nav.NavigateTo("/host");
        }
    }
}