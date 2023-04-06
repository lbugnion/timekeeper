using Markdig.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        public IList<Chat> Chats { get; set; }

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

        public IList<Poll> Polls
        {
            get;
            set;
        }

        public string SecretKey { get; set; }

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

        public void Update(SessionBase session)
        {
            if (!string.IsNullOrEmpty(session.SessionName))
            {
                SessionName = session.SessionName;
            }

            if (session.Polls != null
                && session.Polls.Count > 0)
            {
                Polls = new List<Poll>(session.Polls);

                foreach (var poll in Polls)
                {
                    if (string.IsNullOrEmpty(poll.Uid))
                    {
                        poll.Uid = Guid.NewGuid().ToString();
                    }

                    for (var index = 0; index < poll.Answers.Count; index++)
                    {
                        if (string.IsNullOrEmpty(poll.Answers[index].Letter))
                        {
                            poll.Answers[index].Letter = ((char)('A' + index)).ToString();
                        }
                    }
                }
            }

            if (session.Clocks != null
                && session.Clocks.Count > 0)
            {
                Clocks = new List<Clock>(session.Clocks);

                foreach (var clock in Clocks)
                {
                    clock.ResetDisplay();
                }
            }
        }
    }
}