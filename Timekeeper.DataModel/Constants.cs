namespace Timekeeper.DataModel
{
    public class Constants
    {
        public const string BranchIdKey = "BranchId";
        public const string ClaimsUserIdHeaderKey = "x-timekeeper-claims-userid";
        public const string ConnectMessage = "connect";
        public const string DeleteClockMessage = "delete-clock";
        public const string DisconnectMessage = "disconnect";
        public const string GroupIdHeaderKey = "x-timekeeper-group-id";
        public const string GuestToHostMessageName = "guest-to-host";
        public const string HostToGuestMessageName = "host-to-guest";
        public const string HostToGuestRequestAnnounceMessageName = "host-to-guest-request-announce";
        public const string HubName = "timekeeper";
        public const string StartClockMessageName = "start-clock";
        public const string StopClockMessage = "stop-clock";
        public const string TokenHeaderKey = "x-timekeeper-token";
        public const string UserIdHeaderKey = "x-timekeeper-userid";
        public const string AzureWebJobsStorageVariableName = "AzureWebJobsStorage";
    }
}