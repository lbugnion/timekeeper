using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timekeeper.DataModel;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Timekeeper.Api.Free
{
    public static class SendChat
    {
        [FunctionName(nameof(SendChat))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "post", 
                Route = "chat")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogDebug(requestBody);
            var chat = JsonConvert.DeserializeObject<Chat>(requestBody);

            if (chat.Key == null
                || chat.Key.Length != 10)
            {
                log.LogError("Invalid key");
                return new UnauthorizedObjectResult("Invalid request");
            }

            if (string.IsNullOrEmpty(chat.SenderName)
                || string.IsNullOrEmpty(chat.MessageMarkdown))
            {
                log.LogError("Empty name or message");
                return new BadRequestObjectResult("Empty name or message");
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.ChatMessage,
                    Arguments = new[] { requestBody },
                    GroupName = groupId.ToString()
                });

            log.LogTrace("Chat sent");
            return new OkObjectResult("OK");
        }
    }
}
