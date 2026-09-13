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
                        // build public url using blob client
                        product.ImageUrl = _blobService.GetBlobUri(blobName);
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
                        product.ImageUrl = _blobService.GetBlobUri(blobName);
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
        // removed manual URL construction in favor of ProductBlobService.GetBlobUri
    }
}
