using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using TimeKeeperApi.DataModel;

namespace TimeKeeperApi
{
    public static class StartClock
    {
        [FunctionName(nameof(StartClock))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "start")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> StartClock");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogDebug(requestBody);

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.StartClockMessageName,
                    Arguments = new[] { requestBody }
                });

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}