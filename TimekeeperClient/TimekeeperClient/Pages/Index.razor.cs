using Microsoft.Extensions.Logging;
using System.Reflection;

namespace TimekeeperClient.Pages
{
    public partial class Index
    {
        public const string RedBackgroundClassName = "background-red";
        public const string YellowBackgroundClassName = "background-yellow";
        public const string NormalBackgroundClassName = "background-normal";

        public string ClientVersion
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
            }
            catch
            {
                Log.LogWarning($"Assembly not found");
                ClientVersion = "N/A";
            }
        }
    }
}
