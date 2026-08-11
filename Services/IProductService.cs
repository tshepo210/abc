using System.Collections.Generic;
using System.Threading.Tasks;
using abc.Models;

namespace abc.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductEntity>> GetAllAsync();
        Task<ProductEntity?> GetAsync(string productId);
        Task CreateAsync(ProductEntity product);
        Task UpdateAsync(ProductEntity product);
        Task DeleteAsync(string productId);
    }
}
