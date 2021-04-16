using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class SaveSession
    {
        [FunctionName("SaveSession")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "session/{branchId}/{sessionId}")]
            HttpRequest req,
            string branchId,
            string sessionId,
            [Blob("sessions/{branchId}/{sessionId}.json", FileAccess.Write)]
            Stream sessionBlob,
            ILogger log)
        {
            log.LogInformation("-> SaveSession");

            var success = Guid.TryParse(branchId, out Guid testGuid);

            if (!success
                || testGuid == Guid.Empty)
            {
                log.LogError("Invalid branch ID");
                return new BadRequestObjectResult("Invalid branch ID");
            }

            success = Guid.TryParse(sessionId, out testGuid);

            if (!success
                || testGuid == Guid.Empty)
            {
                log.LogError("Invalid session ID");
                return new BadRequestObjectResult("Invalid session ID");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                log.LogError("No body found");
                return new BadRequestObjectResult("No session found in body");
            }

            var session = JsonConvert.DeserializeObject<SessionBase>(requestBody);

            if (session.BranchId == null
                || session.BranchId.ToLower() != branchId.ToLower())
            {
                log.LogError("Branch IDs don't match");
                return new BadRequestObjectResult("Branch IDs don't match");
            }

            if (session.SessionId == null
                || session.SessionId.ToLower() != sessionId.ToLower())
            {
                log.LogError("Session IDs don't match");
                return new BadRequestObjectResult("Session IDs don't match");
            }

            using (var writer = new StreamWriter(sessionBlob))
            {
                writer.Write(requestBody);
            }

            return new OkObjectResult("Saved to storage");
        }
    }
}
