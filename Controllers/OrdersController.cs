using Microsoft.AspNetCore.Mvc;
using abc.Services;
using abc.Models;

namespace abc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IQueueService _queueService;
        private readonly ITableStorageFunctionClient _tableStorageFunctionClient;

        public OrdersController(
            IProductService productService,
            ICustomerService customerService,
            IQueueService queueService,
            ITableStorageFunctionClient tableStorageFunctionClient)
        {
            _productService = productService;
            _customerService = customerService;
            _queueService = queueService;
            _tableStorageFunctionClient = tableStorageFunctionClient;
        }

        public async Task<IActionResult> Create()
        {
            var vm = new OrderViewModel
            {
                Customers = await _customerService.GetAllAsync(),
                Products = await _productService.GetAllAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Customers = await _customerService.GetAllAsync();
                model.Products = await _productService.GetAllAsync();
                return View(model);
            }

            // Create order message
            var orderMsg = new OrderMessage
            {
                OrderId = System.Guid.NewGuid().ToString(),
                CustomerId = model.CustomerId,
                ProductId = model.ProductId,
                Quantity = model.Quantity
            };

            // Create inventory message (negative delta)
            var invMsg = new InventoryMessage
            {
                ProductId = model.ProductId,
                Delta = -model.Quantity,
                ReferenceId = orderMsg.OrderId
            };

            await _tableStorageFunctionClient.CreateOrderAsync(orderMsg);

            // Enqueue messages (fire-and-forget)
            await _queueService.EnqueueOrderAsync(orderMsg);
            await _queueService.EnqueueInventoryAsync(invMsg);

            // Redirect to products index for now
            return RedirectToAction("Index", "Products");
        }
    }
}
