using System;
using System.Collections.Generic;
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

            branchId = branchId.ToLower();
            var success = Guid.TryParse(branchId, out Guid testGuid);

            if (!success
                || testGuid == Guid.Empty)
            {
                log.LogError("Invalid branch ID");
                return new BadRequestObjectResult("Invalid branch ID");
            }

            var account = CloudStorageAccount.Parse(
                Environment.GetEnvironmentVariable(
                    Constants.AzureWebJobsStorageVariableName));

            var blobClient = account.CreateCloudBlobClient();
            var blobHelper = new BlobHelper(blobClient, null);

            var container = blobHelper.GetContainerFromName("sessions");

            BlobContinuationToken continuationToken = null;
            var result = new List<SessionBase>();

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

            return new OkObjectResult(result);
        }
    }
}

