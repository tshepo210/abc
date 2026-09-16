using System.Net;
using ABCRetail.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetail.Functions.Functions;

public sealed class AzureFilesFunction
{
    private readonly AzureFilesService _storage;

    public AzureFilesFunction(AzureFilesService storage) => _storage = storage;

    [Function("AzureFilesFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "files/{fileName}")] HttpRequestData request,
        string fileName)
    {
        if (request.Body is null || request.Body == Stream.Null)
        {
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        await using var content = new MemoryStream();
        await request.Body.CopyToAsync(content);
        content.Position = 0;
        await _storage.UploadAsync(content, fileName);
        return request.CreateResponse(HttpStatusCode.Created);
    }
}