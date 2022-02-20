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

        public IList<Clock> Clocks
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

        public SessionBase()
        {
            SessionId = Guid.NewGuid().ToString();
            ResetName();
            Clocks = new List<Clock>();
            Polls = new List<Poll>();
        }

        public void ResetName()
        {
            SessionName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public IList<Poll> Polls
        {
            get;
            set;
        }

        public IList<Chat> Chats { get; set; }

        public string SecretKey { get; set; }
    }
}