using System;

namespace abc.Models
{
    public class InventoryMessage
    {
        public string ProductId { get; set; } = string.Empty;
        public int Delta { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        // optional correlation or reference to order
        public string? ReferenceId { get; set; }
    }
}
