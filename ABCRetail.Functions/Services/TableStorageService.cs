using ABCRetail.Functions.Models;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace ABCRetail.Functions.Services;

public sealed class TableStorageService
{
    private readonly TableClient _customers;
    private readonly TableClient _products;

    public TableStorageService(IOptions<AzureStorageOptions> options)
    {
        var storage = options.Value;
        _customers = new TableClient(storage.ConnectionString, storage.TableNameCustomers);
        _products = new TableClient(storage.ConnectionString, storage.TableNameProducts);
    }

    public async Task UpsertCustomerAsync(CustomerEntity customer)
    {
        await _customers.CreateIfNotExistsAsync();
        customer.PartitionKey = "CUSTOMER";
        await _customers.UpsertEntityAsync(customer);
    }

    public async Task UpsertProductAsync(ProductEntity product)
    {
        await _products.CreateIfNotExistsAsync();
        product.PartitionKey = "PRODUCT";
        await _products.UpsertEntityAsync(product);
    }

    public async Task UpsertOrderAsync(OrderEntity order)
    {
        await _products.CreateIfNotExistsAsync();
        order.PartitionKey = "ORDER";
        order.RowKey = string.IsNullOrWhiteSpace(order.RowKey) ? order.OrderId : order.RowKey;
        await _products.UpsertEntityAsync(order);
    }

    public async Task<IReadOnlyList<CustomerEntity>> GetCustomersAsync()
    {
        var results = new List<CustomerEntity>();
        await foreach (var customer in _customers.QueryAsync<CustomerEntity>("PartitionKey eq 'CUSTOMER'"))
        {
            results.Add(customer);
        }

        return results;
    }

    public async Task<IReadOnlyList<ProductEntity>> GetProductsAsync()
    {
        var results = new List<ProductEntity>();
        await foreach (var product in _products.QueryAsync<ProductEntity>("PartitionKey eq 'PRODUCT'"))
        {
            results.Add(product);
        }

        return results;
    }
}