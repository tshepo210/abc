using System;
using Azure;
using Azure.Data.Tables;

namespace abc.Models
{
    public class CustomerEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "CUSTOMER";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        // Business properties
        public string CustomerId
        {
            get => RowKey;
            set => RowKey = value;
        }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public CustomerEntity() { }
    }
}
