using Homework.DTOs.Discount;
using Homework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class DiscountService : IDiscountService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public DiscountService(ApplicationDbContext context, IMemoryCache cache, IConfiguration configuration)
    {
        _context = context;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<Discount> CreateDiscountAsync(AddDiscount discount)
    {
        Discount _discount = new()
        { ProductID = discount.productID,
          ProductName = discount.productName};
        _discount.Available = discount.available;
        _discount.DiscountPercentile = discount.discountPercentile ?? 0;
        _discount.DiscountFlat = discount.discountFlat ?? 0;
        _discount.IsFlat = !(discount.discountFlat == null);
        _context.Discounts.Add(_discount);
        await _context.SaveChangesAsync();
        return _discount;
    }

    public async Task<List<Discount>> GetDiscountsAsync()
    {
        if (!_cache.TryGetValue(_configuration["AppSettings:Discount"] + "All", out List<Discount> discounts))
        {
            discounts = await _context.Discounts.ToListAsync();

            var CacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(20));
            if (discounts.Count > 0)
            {
                _cache.Set(_configuration["AppSettings:Discount"] + "All", discounts, CacheEntryOptions);
            }
        }

        return discounts;
    }

    public async Task<Discount> GetDiscountByIdAsync(int id)
    {
        if (!_cache.TryGetValue(_configuration["AppSettings:Discount"] + id.ToString(), out Discount discount))
        {
            discount = await _context.Discounts.FindAsync(id);

            var CacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));
            _cache.Set(_configuration["AppSettings:Discount"] + id.ToString(), discount, CacheEntryOptions);
        }

        if (discount == null)
        {
            throw new KeyNotFoundException("Discount not found");
        }
        return discount;
    }

    public async Task<bool> UpdateDiscountAsync(Discount discount)
    {
        var existingDiscount = await _context.Discounts.FindAsync(discount.Id);
        if (existingDiscount == null)
        {
            throw new KeyNotFoundException("Discount not found");
        }

        if (existingDiscount != null)
        {
            existingDiscount = discount;
            _context.Entry(existingDiscount).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _cache.Remove(_configuration["AppSettings:Discount"] + discount.Id.ToString());
            _cache.Remove(_configuration["AppSettings:Discount"] + "All");
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteDiscountAsync(int id)
    {
        var discount = await _context.Discounts.FindAsync(id);
        if (discount == null)
        {
            throw new KeyNotFoundException("Discount not found");
        }

        _context.Discounts.Remove(discount);
        await _context.SaveChangesAsync();
        _cache.Remove(_configuration["AppSettings:Discount"] + discount.Id.ToString());
        _cache.Remove(_configuration["AppSettings:Discount"] + "All");
        return true;
    }

    public async Task<List<Discount>> GetDiscountByProductIdAsync(int productId)
    {
        return await _context.Discounts
            .Where(d => d.ProductID == productId && d.Available)
            .ToListAsync();
    }
}
