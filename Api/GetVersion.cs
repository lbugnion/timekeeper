using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Timekeeper
{
    public static class GetVersion
    {
        [FunctionName("GetVersion")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "get",
                Route = "version")] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult("0.5.8888.1");
        }
    }
}
