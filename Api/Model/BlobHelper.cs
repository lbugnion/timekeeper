using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using System;

namespace Timekeeper.Model
{
    public class BlobHelper
    {
        public CloudBlobClient _client;

        public ILogger _logger;

        public BlobHelper(CloudBlobClient client, ILogger logger = null)
        {
            _logger = logger;
            _client = client ?? throw new ArgumentNullException("client");
        }

        public CloudBlobContainer GetContainerFromName(string containerName)
        {
            var container = _client.GetContainerReference(containerName);
            _logger?.LogDebug($"container: {containerName} : {container.Uri}");
            return container;
        }

        public CloudBlobContainer GetContainerFromVariable(string variableName)
        {
            var containerName = Environment.GetEnvironmentVariable(variableName);
            _logger?.LogDebug($"variableName: {variableName} : {containerName}");
            return GetContainerFromName(containerName);
        }
    }
}