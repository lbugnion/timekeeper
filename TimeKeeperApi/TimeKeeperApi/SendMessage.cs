using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeKeeperApi.DataModel;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace TimeKeeperApi
{
    public static class SendMessage
    {
        [FunctionName(nameof(SendMessage))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "post", 
                Route = "send")] 
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> SendMessage");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogDebug(requestBody);

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.HostToGuestMessageName,
                    Arguments = new [] { requestBody }
                });

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}
