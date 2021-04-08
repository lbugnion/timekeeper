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

        public bool IsHost
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

        public string PeerId
        {
            get;
            set;
        }
    }
}