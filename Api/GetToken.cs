using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class GetToken
    {
        [FunctionName("GetToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "get", 
                Route = "token")]
            HttpRequest req)
        {
            return new OkObjectResult(Environment.GetEnvironmentVariable(Constants.TokenHeaderKey));
        }
    }
}
