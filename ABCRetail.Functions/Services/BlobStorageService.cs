using ABCRetail.Functions.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace ABCRetail.Functions.Services;

public sealed class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IOptions<AzureStorageOptions> options)
    {
        var storage = options.Value;
        _container = new BlobServiceClient(storage.ConnectionString)
            .GetBlobContainerClient(storage.BlobContainer);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType)
    {
        var blobName = $"{Guid.NewGuid():N}-{Path.GetFileName(fileName)}";
        var blob = _container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        return blob.Uri.ToString();
    }
}