using System.Net;
using System.Text.Json;
using ABCRetail.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ABCRetail.Functions.Functions;

public sealed class QueueStorageFunction
{
    private readonly QueueStorageService _storage;

    public QueueStorageFunction(QueueStorageService storage) => _storage = storage;

    [Function("QueueStorageFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "queues/{queueName}")] HttpRequestData request,
        string queueName)
    {
        if (!queueName.Equals("orders", StringComparison.OrdinalIgnoreCase) &&
            !queueName.Equals("inventory", StringComparison.OrdinalIgnoreCase))
        {
            return request.CreateResponse(HttpStatusCode.NotFound);
        }

        if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            using var document = await JsonDocument.ParseAsync(request.Body);
            await _storage.SendAsync(queueName, document.RootElement.GetRawText());
            return request.CreateResponse(HttpStatusCode.Accepted);
        }

        var message = await _storage.ReceiveAsync(queueName);
        var response = request.CreateResponse(message is null ? HttpStatusCode.NoContent : HttpStatusCode.OK);
        if (message is not null)
        {
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(message);
        }

        return response;
    }
}