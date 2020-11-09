using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Limalima.Backend.Azure
{
    public interface IAzureImageUploadComponent
    {
        Task<string> UploadFileToStorage(Stream stream, string fileName);
    }

    public class AzureImageUploadComponent : IAzureImageUploadComponent
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger _logger;

        public AzureImageUploadComponent(IConfiguration config, ILogger<AzureImageUploadComponent> logger)
        {
            _logger = logger;
            string connectionString = config.GetValue<string>("AZURE_STORAGE_CONNECTION_STRING");
            string containerName = config.GetValue<string>("AZURE_STORAGE_CONTAINER");
            
            BlobServiceClient cloudBlobClient = new BlobServiceClient(connectionString);
            _containerClient = cloudBlobClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadFileToStorage(Stream stream, string fileName)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(stream, true);

                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "UploadFileToStorage error");
                return "";
            }
        }

    }
}
