using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

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
