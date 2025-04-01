using Homework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public ProductService(ApplicationDbContext context, IMemoryCache cache, IConfiguration configuration)
    {
        _context = context;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        if (product.Price <= 0)
            throw new ArgumentException("Price must be a positive value.");

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<List<Product>> GetProductsAsync(string name)
    {
        if (!_cache.TryGetValue(_configuration["AppSettings:Product"] + name, out List<Product> products))
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }
            products = await query.ToListAsync();
            var CacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(20));

            _cache.Set(_configuration["AppSettings:Product"] + name, products, CacheEntryOptions);
        }

        return products;
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        if (!_cache.TryGetValue(_configuration["AppSettings:Product"] + id.ToString(), out Product product))
        {
            product = await _context.Products.FindAsync(id);

            var CacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(20));

            _cache.Set(_configuration["AppSettings:Product"] + id.ToString(), product, CacheEntryOptions);
        }
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }
        return product;
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        var existingProduct = await _context.Products.FindAsync(product.Id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        _context.Entry(existingProduct).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        _cache.Remove(_configuration["AppSettings:Product"] + product.Id.ToString());
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _cache.Remove(_configuration["AppSettings:Product"] + id.ToString());
        return true;
    }
}
