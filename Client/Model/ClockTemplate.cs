using System;
using System.Collections.Generic;

namespace Timekeeper.Client.Model
{
    public class ClockDefinition
    {
        public string Color
        {
            get;
            set;
        }

        public TimeSpan Time
        {
            get;
            set;
        }
    }

    public class ClockForTemplate
    {
        public ClockDefinition AlmostDone
        {
            get;
            set;
        }

        public ClockDefinition Countdown
        {
            get;
            set;
        }

        public string Label
        {
            get;
            set;
        }

        public ClockDefinition PayAttention
        {
            get;
            set;
        }
    }

    public class ClockTemplate
    {
        public IList<ClockForTemplate> Clocks
        {
            get;
            set;
        }

        public string SessionId
        {
            get;
            set;
        }

        public string SessionName
        {
            get;
            set;
        }
    }
}