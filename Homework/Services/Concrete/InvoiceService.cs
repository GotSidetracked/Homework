using Homework.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IDiscountService _discountService;

    public InvoiceService(ApplicationDbContext context, IMemoryCache cache,
                          IConfiguration configuration, IDiscountService discountService)
    {
        _context = context;
        _cache = cache;
        _configuration = configuration;
        _discountService = discountService;
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        var product = await _context.Products.FindAsync(invoice.ProductId);

        List<Discount> activeDiscounts;
        if (!_cache.TryGetValue(_configuration["AppSettings:Discount"] + "_Product_" + product.Id.ToString(), out activeDiscounts))
        {
            activeDiscounts = await _discountService.GetDiscountByProductIdAsync(product.Id);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            _cache.Set(_configuration["AppSettings:Discount"] + "_Product_" + product.Id.ToString(), activeDiscounts, cacheEntryOptions);
        }
        decimal finalPrice = product.Price;
        if (activeDiscounts.Count() > 0)
        {
            var bestDiscount = activeDiscounts.OrderByDescending(d =>
                                 d.IsFlat ?
                                    (decimal)(d.DiscountFlat ?? 0) :
                                    (decimal)((product.Price * d.DiscountPercentile ?? 0) / 100)
                                 ).FirstOrDefault();

            if (bestDiscount.IsFlat)
            {
                finalPrice -= bestDiscount.DiscountFlat ?? 0;
            }
            else
            {
                finalPrice -= (finalPrice * (bestDiscount.DiscountPercentile ?? 0) / 100);
            }

            if (finalPrice < 0)
            {
                finalPrice = 0;
            }
            invoice.DiscountID = bestDiscount.Id;
            invoice.DiscountedPrice = finalPrice;

            bestDiscount.UsedAmountOfTimes += 1;
            await _discountService.UpdateDiscountAsync(bestDiscount);
        }

        invoice.Price = finalPrice * invoice.Quantity;
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        _cache.Set(_configuration["AppSettings:Invoice"] + invoice.Id.ToString(), invoice, new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
            .SetSlidingExpiration(TimeSpan.FromMinutes(1)));

        return invoice;
    }

    public async Task<Invoice> GetInvoiceByIdAsync(int id)
    {
        if (!_cache.TryGetValue(_configuration["AppSettings:Invoice"] + id.ToString(), out Invoice invoice))
        {
            invoice = await _context.Invoices.FindAsync(id);

            _cache.Set(_configuration["AppSettings:Invoice"] + id.ToString(), invoice, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(1)));
        }

        return invoice;
    }

    public async Task<List<Invoice>> GetInvoicesAsync()
    {
        var invoices = await _context.Invoices.ToListAsync();
        _cache.Set(_configuration["AppSettings:Invoices"] + "All", invoices, new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
            .SetSlidingExpiration(TimeSpan.FromMinutes(1)));

        return invoices;
    }

    public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
    {
        var existingInvoice = await _context.Invoices.FindAsync(invoice.Id);

        existingInvoice.Quantity = invoice.Quantity;
        existingInvoice.Price = invoice.Price;

        _context.Entry(existingInvoice).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        _cache.Remove(_configuration["AppSettings:Invoice"] + invoice.Id.ToString());

        return true;
    }

    public async Task<bool> DeleteInvoiceAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        _cache.Remove(_configuration["AppSettings:Invoice"] + id.ToString());

        return true;
    }
}
