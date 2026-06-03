using Azure.Storage.Blobs;
using System.Diagnostics;
using System.Text;

namespace AnotadorGymAppApi.Infrastructure.Cache
{
    public class BlobCacheService : ICacheService
    {
        private readonly BlobContainerClient blobContainerClient;
        public BlobCacheService(IConfiguration configuration)
        {
            var connectionString = configuration["Storage:ConnectionString"];
            Debug.WriteLine($"BlobCacheService initialized with connection string: {connectionString}");

            blobContainerClient = new BlobContainerClient(connectionString, "cache");
        }
        public async Task DeleteAsync(string key)
        {
            var blob = blobContainerClient.GetBlobClient(key);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<string?> GetAsync(string key)
        {
            var blob = blobContainerClient.GetBlobClient(key);
            
            if (!await blob.ExistsAsync())
                return null;

            var content = await blob.DownloadContentAsync();
            return content.Value.Content.ToString();
        }

        public async Task SetAsync(string key, string content)
        {
            var blob = blobContainerClient.GetBlobClient(key);            

            await blob.UploadAsync(BinaryData.FromString(content), overwrite: true);
        }
    }
}
