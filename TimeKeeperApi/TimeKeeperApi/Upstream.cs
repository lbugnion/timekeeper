using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using TimeKeeperApi.Model;
using TimeKeeperApi.DataModel;

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

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.DisconnectMessage,
                    Arguments = new[] { invocationContext.UserId }
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
            log.LogInformation($"-> {nameof(ReceiveConnected)}");
            log.LogDebug($"UserId: {invocationContext.UserId}");

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.ConnectMessage,
                    Arguments = new[] { invocationContext.UserId }
                });

            return new OkObjectResult("OK");
        }
    }
}
