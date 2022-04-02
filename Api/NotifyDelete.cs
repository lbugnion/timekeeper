using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using Timekeeper.Model;

namespace Timekeeper
{
    public static class NotifyDelete
    {
        [FunctionName(nameof(NotifyDelete))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "notify-delete/{branchId}/{sessionId}")]
            HttpRequest req,
            string branchId,
            string sessionId,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> NotifyDelete");

            var verificationResult = Verification.Verify(branchId, sessionId, log);

            if (verificationResult != null)
            {
                return verificationResult;
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.NotifyDeleteSessionMessageName,
                    Arguments = new[] { sessionId },
                    GroupName = branchId
                });

            return new OkObjectResult("Sent");
        }
    }
}
