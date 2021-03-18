using Microsoft.AspNetCore.Http;
using System;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class HttpRequestExtensions
    {
        public static Guid GetGroupId(this HttpRequest req)
        {
            var groupId = req.Headers[Constants.GroupIdHeaderKey];
            var success = Guid.TryParse(groupId, out Guid guid);

            if (!success)
            {
                return Guid.Empty;
            }

            return guid;
        }
    }
}