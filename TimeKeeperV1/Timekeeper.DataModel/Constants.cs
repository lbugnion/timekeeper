namespace TimeKeeper.DataModel
{
    public class Constants
    {
        public const string HostToGuestMessageName = "host-to-guest";
        public const string GuestToHostMessageName = "guest-to-host";
        public const string ConnectMessage = "connect";
        public const string DisconnectMessage = "disconnect";
        public const string HubName = "timekeeper";
        public const string StartClockMessageName = "start-clock";
        public const string StopClockMessage = "stop-clock";
        public const string GroupIdHeaderKey = "x-timekeeper-group-id";
        public const string UserIdHeaderKey = "x-timekeeper-userid";
    }
}