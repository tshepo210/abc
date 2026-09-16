using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using abc.Models;
using Microsoft.Extensions.Options;

namespace abc.Services
{
    public class AzureQueueService : IQueueService
    {
        private readonly HttpClient _httpClient;
        private readonly FunctionsOptions _functions;

        public AzureQueueService(
            IOptions<FunctionsOptions> functions,
            HttpClient httpClient)
        {
            _functions = functions.Value;
            _httpClient = httpClient;
        }

        public async Task EnqueueOrderAsync(OrderMessage order)
        {
            if (order == null) return;
            await SendAsync("orders", order);
        }

        public async Task EnqueueInventoryAsync(InventoryMessage msg)
        {
            if (msg == null) return;
            await SendAsync("inventory", msg);
        }

        private async Task SendAsync(string queueName, object message)
        {
            if (string.IsNullOrWhiteSpace(_functions.BaseUrl))
                throw new InvalidOperationException("Functions:BaseUrl is not configured.");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_functions.BaseUrl.TrimEnd('/')}/queues/{queueName}")
            {
                Content = JsonContent.Create(message)
            };
            if (!string.IsNullOrWhiteSpace(_functions.QueueFunctionKey))
                request.Headers.Add("x-functions-key", _functions.QueueFunctionKey);

            using (request)
            using (var response = await _httpClient.SendAsync(request))
            {
                if (response.IsSuccessStatusCode) return;

                var detail = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"QueueStorageFunction returned {(int)response.StatusCode} ({response.ReasonPhrase}). {detail}");
            }
        }
    }
}
