using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Functions.Models;

public sealed class ProductEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "PRODUCT";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Price { get; set; }
    public int StockQuantity { get; set; }
    public string ImageName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}