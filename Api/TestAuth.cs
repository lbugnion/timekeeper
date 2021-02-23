using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Collections.Generic;

namespace Timekeeper.Api.Free
{
    public static class AuthTest
    {
        private class ClientPrincipal
        {
            public string IdentityProvider { get; set; }
            public string UserId { get; set; }
            public string UserDetails { get; set; }
            public IEnumerable<string> UserRoles { get; set; }
        }

        [Authorize]
        [FunctionName("AuthTest")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous, 
            "get", 
            Route = "test")] 
        HttpRequest req,
        ILogger log)
        {
            var principal = new ClientPrincipal();

            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.ASCII.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(
                    json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

            if (!principal.UserRoles?.Any() ?? true)
            {
                return new OkObjectResult(new ClaimsPrincipal());
            }

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            return new OkObjectResult(new ClaimsPrincipal(identity));
        }
    }
}
