using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using abc.Services;
using abc.Models;

namespace abc.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductBlobService _blobService;

        public ProductsController(IProductService productService, IProductBlobService blobService)
        {
            _productService = productService;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _productService.GetAllAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new ProductEntity());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEntity product, Microsoft.AspNetCore.Http.IFormFile? productImage)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(product.ProductId))
                    product.ProductId = System.Guid.NewGuid().ToString();

                // handle image upload if present
                if (productImage != null)
                {
                    var blobName = await _blobService.UploadAsync(productImage, product.ProductId);
                    if (!string.IsNullOrWhiteSpace(blobName))
                    {
                        product.ImageName = blobName;
                        // build public url
                        var opt = ((Microsoft.Extensions.Options.IOptions<abc.Models.AzureStorageOptions>)HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Options.IOptions<abc.Models.AzureStorageOptions>)))!.Value;
                        var accountConn = opt.ConnectionString;
                        // The ProductBlobService sets container public access; blob URL can be obtained from container client
                        // Use ProductBlobService to construct the URL or assume default blob endpoint format is available via BlobServiceClient
                        product.ImageUrl = $"{_blobServiceUrl(blobName)}";
                    }
                }

                await _productService.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var product = await _productService.GetAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProductEntity product, Microsoft.AspNetCore.Http.IFormFile? productImage)
        {
            if (id != product.ProductId) return BadRequest();
            if (ModelState.IsValid)
            {
                if (productImage != null)
                {
                    // delete old image if exists
                    if (!string.IsNullOrWhiteSpace(product.ImageName))
                    {
                        await _blobService.DeleteAsync(product.ImageName);
                    }
                    var blobName = await _blobService.UploadAsync(productImage, product.ProductId);
                    if (!string.IsNullOrWhiteSpace(blobName))
                    {
                        product.ImageName = blobName;
                        product.ImageUrl = $"{_blobServiceUrl(blobName)}";
                    }
                }

                await _productService.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var product = await _productService.GetAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var product = await _productService.GetAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrWhiteSpace(product.ImageName))
                {
                    await _blobService.DeleteAsync(product.ImageName);
                }
            }
            await _productService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // helper to construct blob url via BlobServiceClient knowledge
        private string _blobServiceUrl(string blobName)
        {
            // Try to get container client endpoint from ProductBlobService via reflection-less approach: construct from storage account endpoints
            // For development simplicity, assume container has public URL using standard blob endpoint in connection string
            var opt = (Microsoft.Extensions.Options.IOptions<abc.Models.AzureStorageOptions>)HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Options.IOptions<abc.Models.AzureStorageOptions>));
            var container = opt!.Value.BlobContainer;
            // If connection string contains BlobEndpoint, try to parse account blob endpoint
            var conn = opt.Value.ConnectionString;
            // Attempt to extract AccountName and build URL: https://{accountName}.blob.core.windows.net/{container}/{blobName}
            string accountName = null!;
            try
            {
                var parts = conn.Split(';');
                foreach (var p in parts)
                {
                    if (p.StartsWith("AccountName=", System.StringComparison.OrdinalIgnoreCase))
                    {
                        accountName = p.Substring("AccountName=".Length);
                        break;
                    }
                }
            }
            catch { }
            if (!string.IsNullOrWhiteSpace(accountName))
            {
                return $"https://{accountName}.blob.core.windows.net/{container}/{blobName}";
            }
            // fallback: return blobName only
            return blobName;
        }
    }
}
