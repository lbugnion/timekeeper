using System;
using System.Collections.Generic;
using System.Text;

namespace Timekeeper.DataModel
{
    public class UpdateHostInfo
    {
        public UpdateAction Action
        {
            get;
            set;
        }

        public string SessionName
        {
            get;
            set;
        }

        public StartClockMessage Clock
        {
            get;
            set;
        }

        public string PreviousClockId { get; set; }
    }
}
