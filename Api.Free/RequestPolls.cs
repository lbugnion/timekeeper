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

namespace Timekeeper.Api.Free
{
    public static class RequestPolls
    {
        [FunctionName(nameof(RequestPolls))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "polls")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> RequestPolls");

            try
            {
                var groupId = req.GetGroupId();
                log.LogDebug($"groupId: {groupId}");

                if (groupId == Guid.Empty)
                {
                    log.LogError("No groupId found in headers");
                    return new BadRequestObjectResult("Invalid request");
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                log.LogDebug(requestBody);

                // TODO Is there a way to optimize this and other functions' calls?

                await queue.AddAsync(
                    new SignalRMessage
                    {
                        Target = Constants.RequestPollsMessage,
                        Arguments = new[] { string.Empty },
                        GroupName = groupId.ToString()
                    });
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult($"Issue with the request: {ex.Message}");
            }

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}