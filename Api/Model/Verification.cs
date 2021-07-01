using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Timekeeper.Model
{
    public static class Verification
    {
        public static ObjectResult Verify(
            string branchId,
            string sessionId,
            ILogger log)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                var success = Guid.TryParse(branchId, out Guid testGuid);

                if (!success
                    || testGuid == Guid.Empty)
                {
                    log.LogError("Invalid branch ID");
                    return new BadRequestObjectResult("Invalid branch ID");
                }
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                var success = Guid.TryParse(sessionId, out Guid testGuid);

                if (!success
                    || testGuid == Guid.Empty)
                {
                    log.LogError("Invalid session ID");
                    return new BadRequestObjectResult("Invalid session ID");
                }
            }

            return null;
        }
    }
}
