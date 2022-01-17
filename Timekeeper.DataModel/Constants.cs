namespace Timekeeper.DataModel
{
    public class Constants
    {
        public const string Version = "0.7.0.8888"; // Use x.x.x.8888 for Alpha; Use x.x.x.9999 for Beta

        public const string AzureStorageVariableName = "AzureStorage";
        public const string BranchIdKey = "BranchId";
        public const string ClaimsUserIdHeaderKey = "x-timekeeper-claims-userid";
        public const string ConnectMessage = "connect";
        public const string DeleteClockMessage = "delete-clock";
        public const string DisconnectMessage = "disconnect";
        public const string GroupIdHeaderKey = "x-timekeeper-group-id";
        public const string HostNameFreeKey = "HostNameFree";
        public const string HostNameKey = "HostName";
        public const string HostToPeerMessageName = "host-to-peer";
        public const string HostToPeerRequestAnnounceMessageName = "host-to-peer-request-announce";
        public const string HubName = "timekeeper";
        public const string PeerToHostMessageName = "peer-to-host";
        public const string StartClockMessageName = "start-clock";
        public const string StopClockMessage = "stop-clock";
        public const string TokenHeaderKey = "x-timekeeper-token";
        public const string UpdateHostMessageName = "update-host";
        public const string UserIdHeaderKey = "x-timekeeper-userid";
        public const string NotifyDeleteSessionMessageName = "notify-delete-session";
        public const string PublishPollMessage = "publish-poll";
        public const string UnpublishPollMessage = "unpublish-poll";
        public const string VotePollMessage = "vote-poll";
        public const string ReceivePollsMessage = "receive-polls";
        public const string RequestPollsMessage = "request-polls";
        public const string RolePresenter = "presenter";
        public const string MovePollMessage = "move-poll";
    }
}