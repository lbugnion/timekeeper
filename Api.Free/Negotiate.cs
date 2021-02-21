using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Timekeeper.DataModel;

namespace Timekeeper
{
    // V0.5.0
    public static class Negotiate
    {
        [FunctionName(nameof(Negotiate))]
        public static SignalRConnectionInfo Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "negotiate")]
            HttpRequest req,
            [SignalRConnectionInfo(
                HubName = Constants.HubName,
                UserId = "{headers.x-timekeeper-userid}")]
            SignalRConnectionInfo connectionInfo,
            ILogger log)
        {
            log.LogInformation("-> Negotiate");
            return connectionInfo;
        }
    }
}