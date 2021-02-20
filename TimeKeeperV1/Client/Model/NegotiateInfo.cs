using System;

namespace Model
{
    public class NegotiateInfo
    {
        public string AccessToken
        {
            get;
            set;
        }

        public string HubName
        {
            get
            {
                if (Url == null)
                {
                    return string.Empty;
                }

                var parts = Url.Query.Split(new[]
                {
                '='
            });

                return parts[1];
            }
        }

        public Uri Url
        {
            get;
            set;
        }
    }
}