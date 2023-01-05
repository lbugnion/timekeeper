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
    public static class LikeChat
    {
        [FunctionName(nameof(LikeChat))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "post", 
                Route = "like-chat")]
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
            var message = JsonConvert.DeserializeObject<LikeChatMessage>(requestBody);

            if (message.Peer == null
                || string.IsNullOrEmpty(message.Peer.PeerId))
            {
                log.LogError("Empty Peer ID or message");
                return new BadRequestObjectResult("Empty Peer ID or message");
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.LikeChatMessage,
                    Arguments = new[] { requestBody },
                    GroupName = groupId.ToString()
                });

            log.LogTrace($"Like sent: {message.Peer.PeerId} / {message.MessageId} / {message.IsLiked}");
            return new OkObjectResult($"Like sent: {message.Peer.PeerId} / {message.MessageId} / {message.IsLiked}");
        }
    }
}
