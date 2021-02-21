using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class DeleteClock
    {
        [FunctionName(nameof(DeleteClock))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "post", 
                Route = "delete")] 
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            // TODO Can we rationalize this code with StopClock ?

            log.LogInformation("-> DeleteClock");

            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            string clockId = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogDebug($"clockId: {clockId}");

            if (string.IsNullOrEmpty(clockId))
            {
                log.LogError("No clockId found in body");
                return new BadRequestObjectResult("Invalid request");
            }

            var success = Guid.TryParse(clockId, out Guid clockGuid);

            if (!success)
            {
                log.LogError($"clockId {clockId} is not a GUID");
                return new UnprocessableEntityObjectResult("Invalid clock ID");
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.DeleteClockMessage,
                    Arguments = new object[] { clockId },
                    GroupName = groupId.ToString()
                });

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}
