using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Timekeeper.Api.Free.Model
{
    public class NotificationService
    {
        private const string NotificationsUrl = "https://notificationsendpoint.azurewebsites.net/api/send";
        private const string NotifyLogicAppUrl = "https://prod-13.westus2.logic.azure.com:443/workflows/519c851831124fdca96422e2054c8ac2/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=QzzDBMz_w2TgQZ9Kt0QziJghTJL8e14JZVi9jzqvQ7c";

        public static async Task Notify(
            NotificationInfo info,
            ILogger log = null)
        {
            log?.LogInformation("In NotificationService.Notify");

            // Send message to notification hub

            string when;

            if (info.WhenUtc > DateTime.MinValue)
            {
                when = $" (sent {info.WhenUtc.ToString("yyyy-MM-dd HH:mm:ss")} UTC)";
            }
            else
            {
                when = $" (sent {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")} UTC)";
            }

            info.Message = info.Message == null ? "No message" : info.Message.Replace("\"", "\\\"");

            if (!info.TeamsOnly)
            {
                var json = $"{{\"title\":\"{info.Title}\",\"body\": \"{info.Message}\",\"channel\":\"Docs-Videos\"}}";

                var content = new StringContent(json);
                var request = new HttpRequestMessage(HttpMethod.Post, NotificationsUrl);
                var code = Environment.GetEnvironmentVariable(Constants.NotifyFunctionCodeVariableName);

                if (string.IsNullOrEmpty(code))
                {
                    log?.LogError("Function code not found in NotificationService.Notify");
                    return;
                }

                request.Headers.Add("x-functions-key", code);
                request.Content = content;
                var response = await HttpClientProvider.GetClient().SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                log?.LogDebug($"Sent to NotificationHub: {result}");
            }

            // Send message to Teams

            var infoJson = JsonConvert.SerializeObject(info);

            var queueName = Environment.GetEnvironmentVariable(Constants.QueueNameForTeamsVariableName);
            var queue = new QueueClient(
                Environment.GetEnvironmentVariable(Constants.AzureWebJobsStorageVariableName),
                queueName);

            var queueResponse = queue.SendMessage(infoJson);
            log?.LogDebug($"Sent to Teams Logic App: {queueResponse.Value.MessageId}");
        }
    }
}