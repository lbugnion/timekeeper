using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using TimeKeeperApi.DataModel;

namespace TimeKeeperApi
{
    public static class Negotiate
    {
        [FunctionName(nameof(Negotiate))]
        public static SignalRConnectionInfo Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get", 
                Route = "negotiate")] 
            HttpRequest req,
            [SignalRConnectionInfo(HubName = Constants.HubName)]
            SignalRConnectionInfo connectionInfo,
            ILogger log)
        {
            log.LogInformation("-> Negotiate");
            return connectionInfo;
        }
    }
}
