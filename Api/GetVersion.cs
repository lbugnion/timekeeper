using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Timekeeper
{
    public static class GetVersion
    {
        [FunctionName("GetVersion")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "version")]
            HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult("0.6.0.0");
        }
    }
}