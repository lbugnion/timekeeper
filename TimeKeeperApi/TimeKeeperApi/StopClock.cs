using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TimeKeeperApi.DataModel;

namespace TimeKeeperApi
{
    public static class StopClock
    {
        [FunctionName(nameof(StopClock))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "stop")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> StopClock");

            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.StopClockMessage,
                    Arguments = new object[] { null },
                    GroupName = groupId.ToString()
                });

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}