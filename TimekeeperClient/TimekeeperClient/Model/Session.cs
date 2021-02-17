using Timekeeper.DataModel;

namespace TimekeeperClient.Model
{
    public class Session
    {
        public string SessionName
        {
            get;
            set;
        }

        public string SessionId
        {
            get;
            set;
        }

        public StartClockMessage ClockMessage
        {
            get;
            set;
        }
    }
}
