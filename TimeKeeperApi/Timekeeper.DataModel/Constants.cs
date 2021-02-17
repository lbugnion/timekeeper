namespace TimeKeeperApi.DataModel
{
    public class Constants
    {
        public const string HostToGuestMessageName = "host-to-guest";
        public const string HubName = "timekeeper";
        public const string StartClockMessageName = "start-clock";
        public const string StopClockMessage = "stop-clock";
        public const string GroupIdHeaderKey = "x-timekeeper-group-id";
        public const string UserIdHeaderKey = "x-timekeeper-userid";
    }
}