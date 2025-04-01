using Homework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IInvoiceService _invoiceService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public OrderService(ApplicationDbContext context, IMemoryCache cache, 
                        IConfiguration configuration, IInvoiceService invoiceService)
    {
        _context = context;
        _cache = cache;
        _configuration = configuration;
        _invoiceService = invoiceService;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        decimal price = 0;
        foreach (Invoice inv in order.OrderedItems)
        {
            inv.OrderID = order.Id;
            price += inv.Price * inv.Quantity;
            await _invoiceService.CreateInvoiceAsync(inv);
        }
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        List<Order> orders = await _context.Orders.Include(o => o.OrderedItems).ToListAsync();

        return orders;
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
        if (!_cache.TryGetValue(_configuration["AppSettings:Order"] + id.ToString(), out Order order))
        {
            order = await _context.Orders.Include(o => o.OrderedItems)
                                             .FirstOrDefaultAsync(o => o.Id == id);
            var CacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                .SetSlidingExpiration(TimeSpan.FromMinutes(20));

            _cache.Set(_configuration["AppSettings:Order"] + id.ToString(), order, CacheEntryOptions);
        }


        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        return order;
    }

    public async Task<bool> UpdateOrderAsync(Order order)
    {
        var existingOrder = await _context.Orders.Include(o => o.OrderedItems)
                                                 .FirstOrDefaultAsync(o => o.Id == order.Id);
        if (existingOrder == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        existingOrder.OrderedItems = order.OrderedItems;
        existingOrder.Price = 0;
        foreach (Invoice item in existingOrder.OrderedItems)
        { 
            existingOrder.Price += item.Price * item.Quantity;  
        }
        
        _context.Entry(existingOrder).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        _cache.Remove(_configuration["AppSettings:Order"] + order.Id.ToString());
        return true;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        _cache.Remove(_configuration["AppSettings:Order"] + id.ToString());
        return true;
    }
}
