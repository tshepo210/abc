using ABCRetail.Functions.Models;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;

namespace ABCRetail.Functions.Services;

public sealed class QueueStorageService
{
    private readonly QueueClient _orders;
    private readonly QueueClient _inventory;

    public QueueStorageService(IOptions<AzureStorageOptions> options)
    {
        var storage = options.Value;
        _orders = new QueueClient(storage.ConnectionString, storage.QueueNameOrders);
        _inventory = new QueueClient(storage.ConnectionString, storage.QueueNameInventory);
    }

    public Task SendAsync(string queueName, string messageJson) => GetQueue(queueName).SendMessageAsync(messageJson);

    public async Task<string?> ReceiveAsync(string queueName)
    {
        var message = await GetQueue(queueName).ReceiveMessageAsync();
        if (message.Value is null)
        {
            return null;
        }

        await GetQueue(queueName).DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
        return message.Value.MessageText;
    }

    private QueueClient GetQueue(string queueName) =>
        queueName.Equals("inventory", StringComparison.OrdinalIgnoreCase) ? _inventory : _orders;
}