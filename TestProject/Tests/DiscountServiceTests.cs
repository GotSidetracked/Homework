using Homework.Models;
using Homework.Services;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Homework.DTOs.Discount;

public class DiscountServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly DiscountService _discountService;

    public DiscountServiceTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _mockCache = new Mock<IMemoryCache>();
        _discountService = new DiscountService(_mockContext.Object, _mockCache.Object, new ConfigurationBuilder().Build());
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldCreateDiscount()
    {
        var addDiscount = new AddDiscount { productID = 1, productName = "Product 1", available = true, discountPercentile = 10 };
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _discountService.CreateDiscountAsync(addDiscount);

        Assert.Equal("Product 1", result.ProductName);
        Assert.Equal(10, result.DiscountPercentile);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetDiscountByIdAsync_ShouldReturnDiscount()
    {
        var discount = new Discount { ProductName= "Test_1", Id = 1, ProductID = 1, DiscountPercentile = 10 };
        _mockContext.Setup(c => c.Discounts.FindAsync(1)).ReturnsAsync(discount);

        var result = await _discountService.GetDiscountByIdAsync(1);

        Assert.Equal(discount, result);
    }

    [Fact]
    public async Task GetDiscountsAsync_ShouldReturnListOfDiscounts()
    {
        var discounts = new List<Discount>
        {
            new Discount { ProductName = "Test_1", Id = 1, ProductID = 1, DiscountPercentile = 10 },
            new Discount { ProductName = "Test_2",  Id = 2, ProductID = 2, DiscountPercentile = 15 }
        };
        _mockContext.Setup(c => c.Discounts.ToListAsync(default)).ReturnsAsync(discounts);

        var result = await _discountService.GetDiscountsAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateDiscountAsync_ShouldUpdateDiscount()
    {
        var discount = new Discount {ProductName = "Test_1", Id = 1, ProductID = 1, DiscountPercentile = 10 };
        _mockContext.Setup(c => c.Discounts.FindAsync(1)).ReturnsAsync(discount);
        discount.DiscountPercentile = 20;

        var result = await _discountService.UpdateDiscountAsync(discount);

        Assert.True(result);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteDiscountAsync_ShouldDeleteDiscount()
    {
        var discount = new Discount {ProductName = "Test_1", Id = 1, ProductID = 1, DiscountPercentile = 10 };
        _mockContext.Setup(c => c.Discounts.FindAsync(1)).ReturnsAsync(discount);

        var result = await _discountService.DeleteDiscountAsync(1);

        Assert.True(result);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }
}
