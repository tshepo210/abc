using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abc.Models;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace abc.Services
{
    public class CustomerTableService : ICustomerService
    {
        private readonly TableClient _tableClient;

        public CustomerTableService(IOptions<AzureStorageOptions> options)
        {
            var opt = options.Value;
            var tableName = string.IsNullOrWhiteSpace(opt.TableNameCustomers) ? "CustomerProfiles" : opt.TableNameCustomers;
            _tableClient = new TableClient(opt.ConnectionString, tableName);
            try
            {
                _tableClient.CreateIfNotExists();
            }
            catch { }
        }

        public async Task DeleteAsync(string customerId)
        {
            await _tableClient.DeleteEntityAsync("CUSTOMER", customerId);
        }

        public async Task<CustomerEntity?> GetAsync(string customerId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<CustomerEntity>("CUSTOMER", customerId);
                return response.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<CustomerEntity>> GetAllAsync()
        {
            var results = new List<CustomerEntity>();
            await foreach (var item in _tableClient.QueryAsync<CustomerEntity>(filter: $"PartitionKey eq 'CUSTOMER'"))
            {
                results.Add(item);
            }
            return results.OrderBy(r => r.LastName).ThenBy(r => r.FirstName);
        }

        public async Task UpdateAsync(CustomerEntity customer)
        {
            customer.PartitionKey = "CUSTOMER";
            await _tableClient.UpsertEntityAsync(customer);
        }
    }
}
