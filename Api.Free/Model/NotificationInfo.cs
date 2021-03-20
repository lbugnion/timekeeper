using System;

namespace Timekeeper.Api.Free.Model
{
    public class NotificationInfo
    {
        public bool IsError
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public bool TeamsOnly
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public DateTime WhenUtc
        {
            get;
            set;
        }
    }
}