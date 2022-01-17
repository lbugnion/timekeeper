using System.Collections.Generic;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model.Polls
{
    public class ListOfPolls
    {
        public string CustomTitle { get; set; }

        public List<Poll> Polls { get; set; } = new List<Poll>();

    }
}
