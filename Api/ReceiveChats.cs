using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class ReceiveChats
    {
        [FunctionName(nameof(ReceiveChats))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "post", 
                Route = "chats")] 
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> ReceiveChats");

            try
            {
                var groupId = req.GetGroupId();
                log.LogDebug($"groupId: {groupId}");

                if (groupId == Guid.Empty)
                {
                    log.LogError("No groupId found in headers");
                    return new BadRequestObjectResult("Invalid request");
                }

                // TODO Is there a way to optimize this and other functions' calls?

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                log.LogDebug(requestBody);

                await queue.AddAsync(
                    new SignalRMessage
                    {
                        Target = Constants.ReceiveChatsMessage,
                        Arguments = new[] { requestBody },
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
