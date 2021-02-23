using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Net.Http;

namespace Timekeeper.Api.Free
{
    public static class AuthTest
    {
        [Authorize]
        [FunctionName("AuthTest")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous, 
            "get", 
            Route = "test")] 
        HttpRequest req)
        {
            var client = new HttpClient();
            var json = await client.GetStringAsync("/.auth/me");
            return new OkObjectResult(json);
        }
    }
}
