using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Timekeeper.DataModel;
using System;
using Newtonsoft.Json.Linq;
using TimeKeeperApi.DataModel;
using System.IO;
using Newtonsoft.Json;

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
            var message = JsonConvert.DeserializeObject<StartClockMessage>(requestBody);

            try
            {
                await queue.AddAsync(
                    new SignalRMessage
                    {
                        Target = Constants.StartClockMessageName,
                        Arguments = new object[] { requestBody }
                    });
            }
            catch
            {

            }

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}
