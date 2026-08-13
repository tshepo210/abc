using System.Collections.Generic;

namespace abc.Models
{
    public class OrderViewModel
    {
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;

        // For rendering lists in the view
        public IEnumerable<CustomerEntity>? Customers { get; set; }
        public IEnumerable<ProductEntity>? Products { get; set; }
    }
}
