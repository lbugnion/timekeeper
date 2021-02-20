using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using TimeKeeperApi.DataModel;
using Microsoft.AspNetCore.Components;

namespace TimeKeeperApi
{
    public static class Upstream
    {
        [FunctionName(nameof(ReceiveDisconnected))]
        public static async Task<IActionResult> ReceiveDisconnected(
            [SignalRTrigger(
                hubName: Constants.HubName,
                "connections",  
                "disconnected")] 
            InvocationContext invocationContext,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation($"-> {nameof(ReceiveDisconnected)}");
            log.LogDebug($"UserId: {invocationContext.UserId}");
            return await HandleEvent(invocationContext.Event, invocationContext.UserId, queue);
        }

        private static async Task<IActionResult> HandleEvent(
            string @event, 
            string userId,
            IAsyncCollector<SignalRMessage> queue)
        {
            string target;

            switch (@event)
            {
                case "connected":
                    target = Constants.ConnectMessage;
                    break;

                case "disconnected":
                    target = Constants.DisconnectMessage;
                    break;

                default:
                    return new UnprocessableEntityObjectResult("Unknown event");
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = target,
                    Arguments = new[] { userId }
                });

            return new OkObjectResult("OK");
        }

        [FunctionName(nameof(ReceiveConnected))]
        public static async Task<IActionResult> ReceiveConnected(
            [SignalRTrigger(
                hubName: Constants.HubName,
                "connections",
                "connected")]
            InvocationContext invocationContext,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation($"-> {nameof(ReceiveDisconnected)}");
            log.LogDebug($"UserId: {invocationContext.UserId}");
            return await HandleEvent(invocationContext.Event, invocationContext.UserId, queue);
        }
    }
}
