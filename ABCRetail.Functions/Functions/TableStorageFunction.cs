using System.Net;
using System.Text.Json;
using ABCRetail.Functions.Models;
using ABCRetail.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions.Functions;

public sealed class TableStorageFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly TableStorageService _storage;
    private readonly ILogger<TableStorageFunction> _logger;

    public TableStorageFunction(TableStorageService storage, ILogger<TableStorageFunction> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    [Function("TableStorageFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "tables/{entityType}")] HttpRequestData request,
        string entityType)
    {
        if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (entityType.Equals("customers", StringComparison.OrdinalIgnoreCase))
                {
                    var customer = await request.ReadFromJsonAsync<CustomerEntity>();
                    if (customer is null) return request.CreateResponse(HttpStatusCode.BadRequest);
                    await _storage.UpsertCustomerAsync(customer);
                }
                else if (entityType.Equals("products", StringComparison.OrdinalIgnoreCase))
                {
                    var product = await request.ReadFromJsonAsync<ProductEntity>();
                    if (product is null) return request.CreateResponse(HttpStatusCode.BadRequest);
                    await _storage.UpsertProductAsync(product);
                }
                else if (entityType.Equals("orders", StringComparison.OrdinalIgnoreCase))
                {
                    var order = await request.ReadFromJsonAsync<OrderEntity>();
                    if (order is null || string.IsNullOrWhiteSpace(order.OrderId))
                    {
                        return request.CreateResponse(HttpStatusCode.BadRequest);
                    }

                    order.RowKey = order.OrderId;
                    await _storage.UpsertOrderAsync(order);
                }
                else
                {
                    return request.CreateResponse(HttpStatusCode.NotFound);
                }

                _logger.LogInformation("Stored {EntityType} data in Azure Table Storage", entityType);
                return request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Invalid JSON received for {EntityType}", entityType);
                var error = request.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteAsJsonAsync(new
                {
                    error = "The request body is not valid JSON for the requested entity type."
                });
                return error;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to store {EntityType} data in Azure Table Storage", entityType);
                var error = request.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteAsJsonAsync(new
                {
                    error = "Azure Table Storage write failed.",
                    detail = exception.Message
                });
                return error;
            }
        }

        object result;
        if (entityType.Equals("customers", StringComparison.OrdinalIgnoreCase))
        {
            result = await _storage.GetCustomersAsync();
        }
        else if (entityType.Equals("products", StringComparison.OrdinalIgnoreCase))
        {
            result = await _storage.GetProductsAsync();
        }
        else
        {
            return request.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}