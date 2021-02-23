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

namespace Timekeeper.Api.Free
{
    public static class AuthTest
    {
        [Authorize]
        [FunctionName("AuthTest")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test")] HttpRequest req)
        {
            // Current user identity returned
            // (quickly thrown together as you get a circular reference error when converting to JSON):
            return new OkObjectResult(
               req.HttpContext.User.Identities.Select(x =>
                  new {
                      Claims = x.Claims.Select(y => new
                      {
                          y.Type,
                          y.Value,
                          y.Issuer,
                          y.Properties
                      }),
                      x.Name,
                      x.Actor,
                      x.AuthenticationType,
                      x.NameClaimType
                  })
            );
        }
    }
}
