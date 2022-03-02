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
    public static class PostChats
    {
        [FunctionName(nameof(PostChats))]
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
            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogDebug(requestBody);
            var chats = JsonConvert.DeserializeObject<ListOfChats>(requestBody);

            foreach (var chat in chats.Chats)
            {
                //if (chat.Key == null
                //    || chat.Key.Length != 10)
                //{
                //    log.LogError("Invalid key");
                //    return new UnauthorizedObjectResult("Invalid request");
                //}

                if (string.IsNullOrEmpty(chat.SenderName)
                    || string.IsNullOrEmpty(chat.MessageMarkdown))
                {
                    log.LogError("Empty name or message");
                    return new BadRequestObjectResult("Empty name or message");
                }
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.ReceiveChatsMessage,
                    Arguments = new[] { requestBody },
                    GroupName = groupId.ToString()
                });

            log.LogTrace("Chat sent");
            return new OkObjectResult("OK");
        }
    }
}
