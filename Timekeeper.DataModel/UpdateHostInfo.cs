using System;
using System.Collections.Generic;
using System.Text;

namespace Timekeeper.DataModel
{
    public class UpdateHostInfo
    {
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
    }
}
