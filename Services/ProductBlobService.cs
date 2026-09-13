using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using abc.Models;

namespace abc.Services
{
    public class ProductBlobService : IProductBlobService
    {
        private readonly BlobContainerClient _container;
        private readonly string _connectionString;
        private readonly StorageSharedKeyCredential? _sharedKey;

        public ProductBlobService(IOptions<AzureStorageOptions> options)
        {
            var opt = options.Value;
            var conn = opt.ConnectionString;
            _connectionString = conn;
            var containerName = string.IsNullOrWhiteSpace(opt.BlobContainer) ? "product-images" : opt.BlobContainer;
            var client = new BlobServiceClient(conn);
            _container = client.GetBlobContainerClient(containerName);
            try
            {
                _container.CreateIfNotExists(PublicAccessType.Blob);
            }
            catch { }

            // Try to parse account name/key from connection string to allow SAS generation when public access is disabled
            try
            {
                string? accountName = null;
                string? accountKey = null;
                var parts = conn.Split(';');
                foreach (var p in parts)
                {
                    if (p.StartsWith("AccountName=", System.StringComparison.OrdinalIgnoreCase))
                        accountName = p.Substring("AccountName=".Length);
                    else if (p.StartsWith("AccountKey=", System.StringComparison.OrdinalIgnoreCase))
                        accountKey = p.Substring("AccountKey=".Length);
                }
                if (!string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(accountKey))
                {
                    _sharedKey = new StorageSharedKeyCredential(accountName, accountKey);
                }
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

        public string GetBlobUri(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName)) return string.Empty;
            var blobClient = _container.GetBlobClient(blobName);
            // If we have shared key credentials available, generate a short-lived SAS token so images can be accessed
            try
            {
                if (_sharedKey != null)
                {
                    var sasBuilder = new BlobSasBuilder
                    {
                        BlobContainerName = _container.Name,
                        BlobName = blobName,
                        Resource = "b",
                        ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                    var sas = sasBuilder.ToSasQueryParameters(_sharedKey).ToString();
                    return $"{blobClient.Uri}?{sas}";
                }
            }
            catch
            {
                // fallthrough to return plain uri
            }

            return blobClient.Uri.ToString();
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
