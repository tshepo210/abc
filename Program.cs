using Microsoft.EntityFrameworkCore;
using abc.Data;

var builder = WebApplication.CreateBuilder(args);

var storageConnectionString = builder.Configuration["AzureStorage:ConnectionString"];
if (string.IsNullOrWhiteSpace(storageConnectionString))
{
    throw new InvalidOperationException(
        "AzureStorage:ConnectionString is not configured. Set it with MVC user secrets or the AzureStorage__ConnectionString environment variable.");
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ApplicationDbContext for EF Core migrations and runtime
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Configure Azure Storage options and register table & blob services
builder.Services.Configure<abc.Models.AzureStorageOptions>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.Configure<abc.Models.TableStorageFunctionOptions>(builder.Configuration.GetSection("TableStorageFunction"));
builder.Services.AddSingleton<abc.Services.ICustomerService, abc.Services.CustomerTableService>();
builder.Services.AddHttpClient<abc.Services.ITableStorageFunctionClient, abc.Services.TableStorageFunctionCustomerService>();
builder.Services.AddSingleton<abc.Services.IProductService, abc.Services.ProductTableService>();
builder.Services.AddSingleton<abc.Services.IProductBlobService, abc.Services.ProductBlobService>();
// Register Azure Queue service for order and inventory messaging (Phase 3)
builder.Services.AddSingleton<abc.Services.IQueueService, abc.Services.AzureQueueService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
