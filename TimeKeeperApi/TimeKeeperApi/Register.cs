using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using TimeKeeperApi.DataModel;

namespace TimeKeeperApi
{
    public static class RegisterUsers
    {
        [FunctionName(nameof(Register))]
        public static async Task<IActionResult> Register(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "register")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            log.LogInformation("-> Register");

            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(requestBody);

            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userInfo.UserId,
                    GroupName = groupId.ToString(),
                    Action = GroupAction.Add
                });

            return new OkObjectResult("OK");
        }

        [FunctionName(nameof(Unregister))]
        public static async Task<IActionResult> Unregister(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "unregister")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            log.LogInformation("-> Unregister");

            var groupId = req.GetGroupId();
            log.LogDebug($"groupId: {groupId}");

            if (groupId == Guid.Empty)
            {
                log.LogError("No groupId found in headers");
                return new BadRequestObjectResult("Invalid request");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(requestBody);

            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userInfo.UserId,
                    GroupName = groupId.ToString(),
                    Action = GroupAction.Remove
                });

            return new OkObjectResult("OK");
        }
    }
}