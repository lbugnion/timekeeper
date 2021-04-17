using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timekeeper.DataModel;
using Timekeeper.Model;

namespace Timekeeper
{
    public static class DeleteSession
    {
        [FunctionName(nameof(DeleteSession))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous, 
                "delete", 
                Route = "session/{branchId}/{sessionId}")]
            string branchId,
            string sessionId,
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("-> SaveSession");

            var verificationResult = Verification.Verify(branchId, sessionId, log);

            if (verificationResult != null)
            {
                return verificationResult;
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                log.LogError("No body found");
                return new BadRequestObjectResult("No session found in body");
            }

            try
            {
                var account = CloudStorageAccount.Parse(
                    Environment.GetEnvironmentVariable(
                        Constants.AzureWebJobsStorageVariableName));

                var blobClient = account.CreateCloudBlobClient();
                var blobHelper = new BlobHelper(blobClient, null);

                var container = blobHelper.GetContainerFromName("sessions");

                var blob = container
                    .GetBlockBlobReference($"{branchId}/{sessionId}.json");

                if (blob != null)
                {
                    await blob.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityObjectResult($"Cannot delete {branchId}/{sessionId}: {ex.Message}");
            }

            return new OkObjectResult("Deleted session");
        }
    }
}

