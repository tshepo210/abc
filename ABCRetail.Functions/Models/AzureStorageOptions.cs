namespace ABCRetail.Functions.Models;

public sealed class AzureStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string BlobContainer { get; set; } = "product-images";
    public string TableNameCustomers { get; set; } = "Customers";
    public string TableNameProducts { get; set; } = "Products";
    public string QueueNameOrders { get; set; } = "orders";
    public string QueueNameInventory { get; set; } = "inventory";
    public string FileShareName { get; set; } = "application-logs";
}