using System;

namespace Timekeeper.DataModel
{
    public class StartClockMessage
    {
        public bool BlinkIfOver
        {
            get;
            set;
        }

        public TimeSpan CountDown
        {
            get;
            set;
        }

        public TimeSpan Red
        {
            get;
            set;
        }

        public DateTime ServerTime
        {
            get;
            set;
        }

        public TimeSpan Yellow
        {
            get;
            set;
        }

        public override string ToString()
        {
            return ServerTime.ToString();
        }
    }
}