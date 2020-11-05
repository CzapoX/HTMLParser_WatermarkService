using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Limalima.Backend.Azure
{
    public class AzureImageUploadComponent : IAzureImageUploadComponent
    {
        private readonly BlobContainerClient containerClient;

        public AzureImageUploadComponent(IConfiguration config)
        {
            string connectionString = config.GetValue<string>("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            string containerName = "imagesfolder" + Guid.NewGuid().ToString();
            containerClient = blobServiceClient.CreateBlobContainer(containerName);
        }

        public async Task<bool> UploadFileToStorage(string localFilePath, string fileName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            
            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            return true;
        }

    }
}
