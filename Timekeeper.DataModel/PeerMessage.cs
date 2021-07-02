using Newtonsoft.Json;

namespace Timekeeper.DataModel
{
    public class PeerMessage
    {
        public const string AnonymousName = "Anonymous";

        public string CustomName
        {
            get;
            set;
        }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                var suffix = IsHost ? " (host)" : string.Empty;

                if (string.IsNullOrEmpty(CustomName))
                {
                    return AnonymousName + suffix;
                }

                return CustomName + suffix;
            }
        }

        public bool IsHost
        {
            get;
            set;
        }

        public string PeerId
        {
            get;
            set;
        }
    }
}