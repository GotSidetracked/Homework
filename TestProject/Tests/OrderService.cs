using Homework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

public class OrderServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _mockCache = new Mock<IMemoryCache>();
        _mockConfig = new Mock<IConfiguration>();
        _orderService = new OrderService(_mockContext.Object, _mockCache.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnOrder_WhenOrderIsValid()
    {
        var order = new Order
        {
            Id = 1,
            OrderedItems = { 10,15,20 },
            Price = 100
        };

        _mockContext.Setup(c => c.Orders.AddAsync(It.IsAny<Order>(), default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Order>)null);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _orderService.CreateOrderAsync(order);

        Assert.NotNull(result);
        Assert.Equal(100, result.Price);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        var order = new Order
        {
            Id = 1,
            OrderedItems = { 10, 15, 20 },
            Price = 100
        };

        _mockContext.Setup(c => c.Orders.FindAsync(1)).ReturnsAsync(order);

        var result = await _orderService.GetOrderByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(100, result.Price);
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldReturnTrue_WhenOrderIsUpdated()
    {
        var order = new Order
        {
            Id = 1,
            Price = 100
        };

        _mockContext.Setup(c => c.Orders.FindAsync(1)).ReturnsAsync(order);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _orderService.UpdateOrderAsync(order);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteOrderAsync_ShouldReturnTrue_WhenOrderIsDeleted()
    {
        var order = new Order
        {
            Id = 1,
            OrderedItems = { 10, 15, 20 },
            Price = 100
        };

        _mockContext.Setup(c => c.Orders.FindAsync(1)).ReturnsAsync(order);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _orderService.DeleteOrderAsync(1);

        Assert.True(result);
    }
}
