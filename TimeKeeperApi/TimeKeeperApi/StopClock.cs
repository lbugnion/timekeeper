using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
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

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.StopClockMessage,
                    Arguments = new object[] { null }
                });

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}
