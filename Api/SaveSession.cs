using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                Route = "save-session/{sessionId}")] 
            HttpRequest req,
            string sessionId,
            [Blob("sessions/{sessionId}.json", FileAccess.Write)]
            Stream sessionBlob,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var success = Guid.TryParse(sessionId, out Guid sessionGuid);

            if (!success
                || sessionGuid == Guid.Empty)
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

            if (session.SessionId != sessionId)
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

