using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class GetVersion
    {
        [FunctionName("GetVersion")]
        public static IActionResult Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "version")]
            HttpRequest req)
        {
            return new OkObjectResult(Constants.Version);
        }
    }
}