using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using abc.Models;
using Microsoft.Extensions.Options;

namespace abc.Services
{
    public class TableStorageFunctionCustomerService : ITableStorageFunctionClient
    {
        private readonly HttpClient _httpClient;
        private readonly TableStorageFunctionOptions _options;

        public TableStorageFunctionCustomerService(
            HttpClient httpClient,
            IOptions<TableStorageFunctionOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public Task CreateCustomerAsync(CustomerEntity customer)
        {
            var payload = new
            {
                customer.PartitionKey,
                RowKey = customer.CustomerId,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.Phone,
                customer.Address
            };

            return PostAsync("customers", payload);
        }

        public Task CreateOrderAsync(OrderMessage order)
        {
            var payload = new
            {
                order.OrderId,
                order.CustomerId,
                order.ProductId,
                order.Quantity,
                order.CreatedAtUtc
            };

            return PostAsync("orders", payload);
        }

        private async Task PostAsync(string entityType, object payload)
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
                throw new InvalidOperationException("TableStorageFunction:BaseUrl is not configured.");

            var functionUri = new Uri(
                $"{_options.BaseUrl.TrimEnd('/')}/tables/{entityType}",
                UriKind.Absolute);
            using var request = new HttpRequestMessage(HttpMethod.Post, functionUri)
            {
                Content = JsonContent.Create(payload)
            };
            if (!string.IsNullOrWhiteSpace(_options.FunctionKey))
                request.Headers.Add("x-functions-key", _options.FunctionKey);

            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return;

            var detail = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"TableStorageFunction returned {(int)response.StatusCode} ({response.ReasonPhrase}). {detail}");
        }
    }
}
