using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using TimeKeeperApi.DataModel;

namespace TimeKeeperApi
{
    public static class Register
    {
        [FunctionName("Register")]
        public static async Task<IActionResult> RunRegister(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "register")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userInfo = JsonConvert.DeserializeObject<GroupInfo>(requestBody);

            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userInfo.UserId,
                    GroupName = userInfo.GroupId,
                    Action = GroupAction.Add
                });

            return new OkObjectResult("OK");
        }

        [FunctionName("Unregister")]
        public static async Task<IActionResult> RunUnregister(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "unregister")]
            HttpRequest req,
            [SignalR(HubName = Constants.HubName)]
            IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userInfo = JsonConvert.DeserializeObject<GroupInfo>(requestBody);

            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userInfo.UserId,
                    GroupName = userInfo.GroupId,
                    Action = GroupAction.Remove
                });

            return new OkObjectResult("OK");
        }
    }
}