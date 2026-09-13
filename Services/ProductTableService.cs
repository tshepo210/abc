using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abc.Models;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace abc.Services
{
    public class ProductTableService : IProductService
    {
        private readonly TableClient _tableClient;

        public ProductTableService(IOptions<AzureStorageOptions> options)
        {
            var opt = options.Value;
            var tableName = string.IsNullOrWhiteSpace(opt.TableNameProducts) ? "Products" : opt.TableNameProducts;
            _tableClient = new TableClient(opt.ConnectionString, tableName);
            try
            {
                _tableClient.CreateIfNotExists();
            }
            catch { }
        }

        public async Task CreateAsync(ProductEntity product)
        {
            if (string.IsNullOrWhiteSpace(product.RowKey))
                product.RowKey = Guid.NewGuid().ToString();

            product.PartitionKey = "PRODUCT";
            await _tableClient.AddEntityAsync(product);
        }

        public async Task DeleteAsync(string productId)
        {
            await _tableClient.DeleteEntityAsync("PRODUCT", productId);
        }

        public async Task<ProductEntity?> GetAsync(string productId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<ProductEntity>("PRODUCT", productId);
                return response.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<ProductEntity>> GetAllAsync()
        {
            var results = new List<ProductEntity>();
            await foreach (var item in _tableClient.QueryAsync<ProductEntity>(filter: $"PartitionKey eq 'PRODUCT'"))
            {
                results.Add(item);
            }
            return results.OrderBy(r => r.Name);
        }

        public async Task UpdateAsync(ProductEntity product)
        {
            product.PartitionKey = "PRODUCT";
            await _tableClient.UpsertEntityAsync(product);
        }
    }
}
