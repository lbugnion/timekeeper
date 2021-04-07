using Newtonsoft.Json;

namespace Timekeeper.DataModel
{
    public class GuestMessage
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
                if (string.IsNullOrEmpty(CustomName))
                {
                    return AnonymousName;
                }

                return CustomName;
            }
        }

        public string GuestId
        {
            get;
            set;
        }
    }
}