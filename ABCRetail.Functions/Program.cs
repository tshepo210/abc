using ABCRetail.Functions.Models;
using ABCRetail.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var storageConfiguration = context.Configuration.GetSection("AzureStorage");
        var connectionString = storageConfiguration["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "AzureStorage__ConnectionString is not configured for the Function App.");
        }

        if (string.IsNullOrWhiteSpace(storageConfiguration["TableNameCustomers"]) ||
            string.IsNullOrWhiteSpace(storageConfiguration["TableNameProducts"]))
        {
            throw new InvalidOperationException(
                "AzureStorage__TableNameCustomers and AzureStorage__TableNameProducts must be configured for the Function App.");
        }

        services.Configure<AzureStorageOptions>(context.Configuration.GetSection("AzureStorage"));
        services.AddSingleton<TableStorageService>();
        services.AddSingleton<BlobStorageService>();
        services.AddSingleton<QueueStorageService>();
        services.AddSingleton<AzureFilesService>();
    })
    .Build();

host.Run();