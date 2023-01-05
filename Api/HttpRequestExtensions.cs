using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
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

        public static bool VerifyToken(this HttpRequest req)
        {
            var token = req.Headers[Constants.TokenHeaderKey].FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var correctToken = Environment.GetEnvironmentVariable(Constants.TimekeeperTokenVariableName);

            return correctToken == token;
        }
    }
}