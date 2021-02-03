using System;

namespace Model
{
    public class NegotiateInfo
    {
        public Uri Url
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

        public string AccessToken
        {
            get;
            set;
        }
    }
}