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
            // For now we only update the SessionName and the polls

            if (!string.IsNullOrEmpty(session.SessionName))
            {
                SessionName = session.SessionName;
            }

            if (session.Polls != null)
            {
                foreach (var poll in session.Polls)
                {
                    var existingPoll = Polls.FirstOrDefault(p => p.Uid == poll.Uid);

                    if (existingPoll != null)
                    {
                        existingPoll.Update(poll);
                    }
                    else
                    {
                        Polls.Add(poll);
                    }
                }
            }

            // TODO Also update the clocks

            //if (session.Clocks != null)
            //{
            //    foreach (var clock in session.Clocks)
            //    {
            //        var existingClock = Clocks.FirstOrDefault(c => c.Message.ClockId == clock.Message.ClockId);

            //        if (existingClock != null)
            //        {
            //            existingClock.Update(clock);
            //        }
            //        else
            //        {
            //            Clocks.Add(clock);
            //        }
            //    }
            //}
        }
    }
}