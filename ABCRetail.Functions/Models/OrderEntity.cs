using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Functions.Models;

public sealed class OrderEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "ORDER";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}
