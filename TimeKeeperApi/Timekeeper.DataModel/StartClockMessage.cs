using System;

namespace Timekeeper.DataModel
{
    public class StartClockMessage
    {
        public DateTime ServerTime
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
