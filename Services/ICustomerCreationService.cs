using System.Threading.Tasks;
using abc.Models;

namespace abc.Services
{
    public interface ITableStorageFunctionClient
    {
        Task CreateCustomerAsync(CustomerEntity customer);
        Task CreateOrderAsync(OrderMessage order);
    }
}
