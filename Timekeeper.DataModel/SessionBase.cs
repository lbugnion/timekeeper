using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Timekeeper.DataModel
{
    public class SessionBase
    {
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

        [Required]
        public string UserId
        {
            get;
            set;
        }

        public SessionBase()
        {
            SessionId = Guid.NewGuid().ToString();
            ResetName();
            UserId = Guid.NewGuid().ToString();
            UserName = GuestMessage.AnonymousName;
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

        public string UserName
        {
            get;
            set;
        }
    }
}