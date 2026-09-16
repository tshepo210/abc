using ABCRetail.Functions.Models;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Options;

namespace ABCRetail.Functions.Services;

public sealed class AzureFilesService
{
    private readonly ShareClient _share;

    public AzureFilesService(IOptions<AzureStorageOptions> options)
    {
        var storage = options.Value;
        _share = new ShareClient(storage.ConnectionString, storage.FileShareName);
    }

    public async Task UploadAsync(Stream content, string fileName)
    {
        var file = _share.GetRootDirectoryClient().GetFileClient(Path.GetFileName(fileName));
        await file.CreateAsync(content.Length);
        await file.UploadAsync(content);
    }
}