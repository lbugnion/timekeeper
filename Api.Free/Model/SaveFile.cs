using System;
using System.Collections.Generic;
using System.Text;

namespace Timekeeper.Api.Free.Model
{
    public class SaveFile
    {
        public bool MustSave
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public string Content
        {
            get;
            set;
        }
    }
}
