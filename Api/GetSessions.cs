using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timekeeper.DataModel;
using Timekeeper.Model;

namespace Timekeeper
{
    public static class GetSessions
    {
        [FunctionName(nameof(GetSessions))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "sessions/{branchId}")]
            HttpRequest req,
            string branchId,
            ILogger log)
        {
            log.LogInformation($"-> {nameof(GetSessions)}");

            var verificationResult = Verification.Verify(branchId, null, log);

            if (verificationResult != null)
            {
                return verificationResult;
            }

            branchId = branchId.ToLower();

            var account = CloudStorageAccount.Parse(
                Environment.GetEnvironmentVariable(
                    Constants.AzureStorageVariableName));

            var blobClient = account.CreateCloudBlobClient();
            var blobHelper = new BlobHelper(blobClient, null);

            var container = blobHelper.GetContainerFromName("sessions");

            var result = new List<SessionBase>();

            try
            {
                BlobContinuationToken continuationToken = null;

                do
                {
                    var response = await container.ListBlobsSegmentedAsync(prefix: "", useFlatBlobListing: true, BlobListingDetails.None, 5000, continuationToken, new BlobRequestOptions(), new OperationContext());
                    continuationToken = response.ContinuationToken;

                    foreach (CloudBlockBlob blob in response.Results)
                    {
                        if (blob.Name.ToLower().Contains(branchId))
                        {
                            var content = await blob.DownloadTextAsync();
                            var session = JsonConvert.DeserializeObject<SessionBase>(content);
                            result.Add(session);
                        }
                    }
                }
                while (continuationToken != null);
            }
            catch (Exception ex)
            {
                log.LogWarning($"Error when loading sessions {ex.Message}");
            }

            return new OkObjectResult(result);
        }
    }
}
