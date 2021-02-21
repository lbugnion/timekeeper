﻿using Newtonsoft.Json;

namespace Timekeeper.DataModel
{
    public class GuestMessage
    {
        public string GuestId
        {
            get;
            set;
        }

        public string CustomName
        {
            get;
            set;
        }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(CustomName))
                {
                    return "Anonymous";
                }

                return CustomName;
            }
        }
    }
}