using System.Collections.Generic;
using System.Threading.Tasks;
using abc.Models;

namespace abc.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerEntity>> GetAllAsync();
        Task<CustomerEntity?> GetAsync(string customerId);
        Task UpdateAsync(CustomerEntity customer);
        Task DeleteAsync(string customerId);
    }
}
