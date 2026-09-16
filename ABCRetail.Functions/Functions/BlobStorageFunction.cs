using System.Net;
using ABCRetail.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetail.Functions.Functions;

public sealed class BlobStorageFunction
{
    private readonly BlobStorageService _storage;

    public BlobStorageFunction(BlobStorageService storage) => _storage = storage;

    [Function("BlobStorageFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "blobs/{fileName}")] HttpRequestData request,
        string fileName)
    {
        if (request.Body is null || request.Body == Stream.Null)
        {
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var contentType = request.Headers.TryGetValues("Content-Type", out var values)
            ? values.FirstOrDefault() ?? "application/octet-stream"
            : "application/octet-stream";
        var uri = await _storage.UploadAsync(request.Body, fileName, contentType);
        var response = request.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(new { blobName = Path.GetFileName(fileName), uri });
        return response;
    }
}