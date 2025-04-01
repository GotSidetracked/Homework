using Microsoft.AspNetCore.Mvc;
using Homework.Models;
using Homework.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace Homework.Controllers
{
    [Route("api/Report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IInvoiceService _invoiceService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IDiscountService _discountService;
        public ReportController(IOrderService orderService, IInvoiceService invoiceService, 
                        IMemoryCache cache, IConfiguration configuration, IDiscountService discountService)
        {
            _orderService = orderService;
            _invoiceService = invoiceService;
            _cache = cache;
            _configuration = configuration;
            _discountService = discountService;
        }

        [HttpGet("Order")]
        public async Task<IActionResult> GetOrderReport(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                var invoices = await _invoiceService.GetInvoicesAsync();
                var orderInvoices = invoices.Where(i => i.OrderID == orderId).ToList();

                var reportData = new
                {
                    OrderId = order.Id,
                    OrderPrice = order.Price,
                    OrderedItems = order.OrderedItems,
                    Invoices = orderInvoices.Select(i => new
                    {
                        i.ProductId,
                        i.Quantity,
                        i.Price,
                        i.DiscountedPrice
                    })
                };

                return Ok(reportData);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }
        }

        [HttpGet("Discounts")]
        public async Task<IActionResult> GetDiscountReport(int discountId)
        {
            try
            {
                var discount = await _discountService.GetDiscountByIdAsync(discountId);

                var invoices = await _invoiceService.GetInvoicesAsync();
                var orderInvoices = invoices.Where(i => i.DiscountID == discountId).ToList();

                decimal totalDiscount = 0;
                decimal percentOff = 0;
                foreach (var invoice in orderInvoices)
                {
                    totalDiscount = invoice.DiscountedPrice * invoice.Quantity ?? 1;
                    if (discount.IsFlat)  {
                        decimal temp = (decimal)(discount.DiscountFlat ?? 0 + invoice.DiscountedPrice ?? 0);
                        if(temp != 0)
                            percentOff = 100 / (temp / discount.DiscountFlat ?? 1);
                    }
                    else {
                        percentOff = discount.DiscountPercentile ?? 0;
                    }
                }
                
                var reportData = new
                {
                    DiscountId = discount.Id,
                    Discounted = discount,
                    DiscountTotal = totalDiscount,
                    TotalOrders = orderInvoices.Count,
                    percentOff = percentOff
                };

                return Ok(reportData);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Discount with ID {discountId} not found.");
            }
        }

    }
}
