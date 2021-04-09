using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timekeeper.DataModel;

namespace Timekeeper
{
    public static class UpdateHost
    {
        [FunctionName(nameof(UpdateHost))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "post", 
                Route = "update")] 
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRMessage> queue,
            ILogger log)
        {
            log.LogInformation("-> UpdateHost");

            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogDebug(requestBody);

            try
            {
                var info = JsonConvert.DeserializeObject<UpdateHostInfo>(requestBody);

                if (info.Clock == null
                    && string.IsNullOrEmpty(info.SessionName))
                {
                    log.LogError($"No information found in UpdateHostInfo");
                    return new UnprocessableEntityObjectResult("No information found");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Issue deserializing: {ex.Message}");
                return new UnprocessableEntityObjectResult("Unable to process");
            }

            await queue.AddAsync(
                new SignalRMessage
                {
                    Target = Constants.UpdateHostMessageName,
                    Arguments = new[] { requestBody },
                    GroupName = groupId.ToString()
                });

            log.LogTrace("Sent");
            return new OkObjectResult("OK");
        }
    }
}

