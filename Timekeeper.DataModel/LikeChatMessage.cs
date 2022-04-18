namespace Timekeeper.DataModel
{
    public class LikeChatMessage
    {
        public PeerMessage Peer { get; set; }

        public string MessageId { get; set; }

        public bool IsLiked { get; set; }

        public string Key { get; set; }
    }
}
