using System.Text.Json;
using System.Threading.Tasks;
using abc.Models;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;

namespace abc.Services
{
    public class AzureQueueService : IQueueService
    {
        private readonly QueueClient _ordersQueue;
        private readonly QueueClient _inventoryQueue;

        public AzureQueueService(IOptions<Models.AzureStorageOptions> options)
        {
            var opt = options.Value;
            var conn = opt.ConnectionString;
            var ordersName = string.IsNullOrWhiteSpace(opt.QueueNameOrders) ? "orders" : opt.QueueNameOrders;
            var inventoryName = string.IsNullOrWhiteSpace(opt.QueueNameInventory) ? "inventory" : opt.QueueNameInventory;

            _ordersQueue = new QueueClient(conn, ordersName);
            _inventoryQueue = new QueueClient(conn, inventoryName);

            try
            {
                _ordersQueue.CreateIfNotExists();
            }
            catch { }

            try
            {
                _inventoryQueue.CreateIfNotExists();
            }
            catch { }
        }

        public async Task EnqueueOrderAsync(OrderMessage order)
        {
            if (order == null) return;
            var json = JsonSerializer.Serialize(order);
            await _ordersQueue.SendMessageAsync(json);
        }

        public async Task EnqueueInventoryAsync(InventoryMessage msg)
        {
            if (msg == null) return;
            var json = JsonSerializer.Serialize(msg);
            await _inventoryQueue.SendMessageAsync(json);
        }
    }
}
