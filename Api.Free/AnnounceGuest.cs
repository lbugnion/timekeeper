using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class AnnounceGuest
    {
        [FunctionName("AnnounceGuest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "announce")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> AnnounceGuest");

            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogDebug(requestBody);

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.GuestToHostMessageName,
                    Arguments = new[] { requestBody },
                    GroupName = groupId.ToString()
                });

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}