using System;
using System.Collections.Generic;

namespace Timekeeper.Client.Model
{
    public class ClockTemplate
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

        public IList<ClockForTemplate> Clocks
        {
            get;
            set;
        }
    }

    public class ClockForTemplate
    {
        public string Label
        {
            get;
            set;
        }

        public ClockDefinition Countdown
        {
            get;
            set;
        }

        public ClockDefinition PayAttention
        {
            get;
            set;
        }

        public ClockDefinition AlmostDone
        {
            get;
            set;
        }
    }

    public class ClockDefinition
    {
        public TimeSpan Time
        {
            get;
            set;
        }

        public string Color
        {
            get;
            set;
        }
    }
}
