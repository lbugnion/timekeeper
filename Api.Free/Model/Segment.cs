using System;
using System.Collections.Generic;
using System.Text;

namespace Timekeeper.Api.Free.Model
{
    public class Segment
    {
        public string Episode
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public IList<string> Hosts
        {
            get;
            set;
        }

        public string Rank
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
    }
}
