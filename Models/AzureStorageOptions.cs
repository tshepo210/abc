namespace abc.Models
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string CustomersTableName { get; set; } = "CustomerProfiles";
        public string ProductsTableName { get; set; } = "Products";
    }
}
