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
            ILogger log)
        {
            log.LogInformation($"-> {nameof(ReceiveDisconnected)}");

            log.LogDebug($"connectionId: {invocationContext.ConnectionId}");

            return new OkObjectResult("OK");
        }

        [FunctionName(nameof(ReceiveConnected))]
        public static async Task<IActionResult> ReceiveConnected(
            [SignalRTrigger(
                hubName: Constants.HubName,
                "connections",
                "connected")]
            InvocationContext invocationContext,
            ILogger log)
        {
            log.LogInformation($"-> {nameof(ReceiveConnected)}");

            log.LogDebug($"connectionId: {invocationContext.ConnectionId}");

            return new OkObjectResult("OK");
        }
    }
}
