using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using abc.Models;

namespace abc.Services
{
    public class ProductBlobService : IProductBlobService
    {
        private readonly BlobContainerClient _container;

        public ProductBlobService(IOptions<AzureStorageOptions> options)
        {
            var opt = options.Value;
            var conn = opt.ConnectionString;
            var containerName = string.IsNullOrWhiteSpace(opt.BlobContainer) ? "product-images" : opt.BlobContainer;
            var client = new BlobServiceClient(conn);
            _container = client.GetBlobContainerClient(containerName);
            try
            {
                _container.CreateIfNotExists(PublicAccessType.Blob);
            }
            catch { }
        }

        public async Task<string?> UploadAsync(IFormFile file, string blobNamePrefix = "")
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName);
            var blobName = string.IsNullOrWhiteSpace(blobNamePrefix)
                ? $"{Guid.NewGuid()}{ext}"
                : $"{blobNamePrefix}_{Guid.NewGuid()}{ext}";

            var blobClient = _container.GetBlobClient(blobName);
            var headers = new BlobHttpHeaders { ContentType = file.ContentType };

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, headers);
            }

            return blobName;
        }

        public async Task<bool> DeleteAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName)) return false;
            try
            {
                var blob = _container.GetBlobClient(blobName);
                await blob.DeleteIfExistsAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
