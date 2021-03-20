using System;
using System.Linq;

namespace Timekeeper.Api.Free.Model
{
    public class Minute
    {
        public static Minute Parse(string minuteString)
        {
            var elements = minuteString.Split(new char[]
                {
                    '|'
                }, StringSplitOptions.RemoveEmptyEntries);

            var firstElement = elements.FirstOrDefault();

            if (firstElement == null)
            {
                return null;
            }


        }
    }
}