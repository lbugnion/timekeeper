using System;

namespace Timekeeper.Model
{
    public static class Verification
    {
        public static string Verify(
            string branchId,
            string sessionId)
        {
            if (!string.IsNullOrEmpty(branchId))
            {
                var success = Guid.TryParse(branchId, out Guid testGuid);

                if (!success
                    || testGuid == Guid.Empty)
                {
                    return "Invalid branch ID";
                }
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                var success = Guid.TryParse(sessionId, out Guid testGuid);

                if (!success
                    || testGuid == Guid.Empty)
                {
                    return "Invalid session ID";
                }
            }

            return null;
        }
    }
}