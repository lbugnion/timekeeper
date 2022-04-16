namespace Timekeeper.DataModel
{
    public class Constants
    {
        public const string Version = "0.8.0.9999"; // Use x.x.x.8888 for Alpha; Use x.x.x.9999 for Beta

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
        public const string MovePollMessage = "move-poll";
        public const string NotifyDeleteSessionMessageName = "notify-delete-session";
        public const string OtherChatContainerCss = "other-chat-container";
        public const string OtherChatCss = "other-chat";
        public const string OtherLikeCss = "other-like";
        public const string OwnChatContainerCss = "own-chat-container";
        public const string OwnChatCss = "own-chat";
        public const string OwnLikeCss = "own-like";
        public const string OwnColor = "#7FC9FF";
        public const string PeerToHostMessageName = "peer-to-host";
        public const string PublishPollMessage = "publish-poll";
        public const string ReceiveChatsMessage = "receive-chats";
        public const string ReceivePollsMessage = "receive-polls";
        public const string RequestChatsMessage = "request-chats";
        public const string RequestPollsMessage = "request-polls";
        public const string ResetPollMessage = "reset-poll";
        public const string RolePresenter = "presenter";
        public const string StartClockMessageName = "start-clock";
        public const string StopClockMessage = "stop-clock";
        public const string TokenHeaderKey = "x-timekeeper-token";
        public const string UnpublishPollMessage = "unpublish-poll";
        public const string UpdateHostMessageName = "update-host";
        public const string UserIdHeaderKey = "x-timekeeper-userid";
        public const string VotePollMessage = "vote-poll";
        public const string You = " (you)";
    }
}