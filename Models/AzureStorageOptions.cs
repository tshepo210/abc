namespace abc.Models
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        // Blob container name for product images
        public string BlobContainer { get; set; } = "product-images";
        // Table names (bind to appsettings keys TableNameCustomers/TableNameProducts)
        public string TableNameCustomers { get; set; } = "CustomerProfiles";
        public string TableNameProducts { get; set; } = "Products";
        // Queue names for orders and inventory messages
        public string QueueNameOrders { get; set; } = "orders";
        public string QueueNameInventory { get; set; } = "inventory";
    }
}
