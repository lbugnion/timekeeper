using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Timekeeper.DataModel
{
    public class SessionBase
    {
        [Required]
        public string BranchId
        {
            get;
            set;
        }

        [Required]
        public string SessionId
        {
            get;
            set;
        }

        [Required]
        public string SessionName
        {
            get;
            set;
        }

        public string LastMessage
        {
            get;
            set;
        }

        public SessionBase()
        {
            SessionId = Guid.NewGuid().ToString();
            ResetName();
            Clocks = new List<Clock>();
        }

        public void ResetName()
        {
            SessionName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public IList<Clock> Clocks
        {
            get;
            set;
        }

        public bool CreatedFromTemplate
        {
            get;
            set;
        }
    }
}