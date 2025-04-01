using Homework.DTOs.Discount;
using Homework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class DiscountServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly DiscountService _discountService;

    public DiscountServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DiscountServiceTestDb")
            .Options;

        _context = new ApplicationDbContext(options);

        _mockCache = new Mock<IMemoryCache>();
        _mockConfiguration = new Mock<IConfiguration>();

        _discountService = new DiscountService(_context, _mockCache.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldCreateDiscount()
    {
        var addDiscount = new AddDiscount
        {
            productID = 1,
            productName = "Test Product",
            available = true,
            discountPercentile = 10,
            discountFlat = 0
        };

        var result = await _discountService.CreateDiscountAsync(addDiscount);

        Assert.NotNull(result);
        Assert.Equal(addDiscount.productID, result.ProductID);
        Assert.Equal(addDiscount.productName, result.ProductName);
        Assert.Equal(addDiscount.discountPercentile, result.DiscountPercentile);
    }

    [Fact]
    public async Task GetDiscountsAsync_ShouldReturnListOfDiscounts()
    {
        var mockDiscounts = new List<Discount>
        {
            new Discount { Id = 1, ProductID = 1, ProductName = "Discount 1" },
            new Discount { Id = 2, ProductID = 2, ProductName = "Discount 2" }
        };
        await _context.Discounts.AddRangeAsync(mockDiscounts);
        await _context.SaveChangesAsync();

        _mockConfiguration.Setup(c => c["AppSettings:Discount"]).Returns("DiscountPrefix");

        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out mockDiscounts)).Returns(false);

        var result = await _discountService.GetDiscountsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Discount 1", result[0].ProductName);
        Assert.Equal("Discount 2", result[1].ProductName);
    }

    [Fact]
    public async Task GetDiscountByIdAsync_ShouldReturnDiscount_WhenFound()
    {
        var mockDiscount = new Discount { Id = 1, ProductID = 1, ProductName = "Discount 1" };
        await _context.Discounts.AddAsync(mockDiscount);
        await _context.SaveChangesAsync();

        _mockConfiguration.Setup(c => c["AppSettings:Discount"]).Returns("DiscountPrefix");

        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out mockDiscount)).Returns(false);

        var result = await _discountService.GetDiscountByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Discount 1", result.ProductName);
    }

    [Fact]
    public async Task GetDiscountByIdAsync_ShouldThrowException_WhenDiscountNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _discountService.GetDiscountByIdAsync(1));
    }

    [Fact]
    public async Task UpdateDiscountAsync_ShouldUpdateDiscount()
    {
        var existingDiscount = new Discount {ProductID = 15, ProductName="Name", Id = 1, DiscountPercentile = 10, DiscountFlat = 0 };
        await _context.Discounts.AddAsync(existingDiscount);
        await _context.SaveChangesAsync();

        var updatedDiscount = new Discount {ProductID = 5,ProductName="Name_1", Id = 1, DiscountPercentile = 20, DiscountFlat = 5 };

        _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

        var result = await _discountService.UpdateDiscountAsync(updatedDiscount);

        Assert.True(result);
        var updated = await _context.Discounts.FindAsync(1);
        Assert.Equal(20, updated.DiscountPercentile);
        Assert.Equal(5, updated.DiscountFlat);
    }

    [Fact]
    public async Task DeleteDiscountAsync_ShouldDeleteDiscount()
    {
        var discountId = 1;
        var discount = new Discount { Id = discountId, ProductID = 1, ProductName = "Discount 1" };
        await _context.Discounts.AddAsync(discount);
        await _context.SaveChangesAsync();

        _mockCache.Setup(c => c.Remove(It.IsAny<object>()));

        var result = await _discountService.DeleteDiscountAsync(discountId);

        Assert.True(result);
        var deletedDiscount = await _context.Discounts.FindAsync(discountId);
        Assert.Null(deletedDiscount);
    }

    [Fact]
    public async Task GetDiscountByProductIdAsync_ShouldReturnDiscounts()
    {
        var productId = 1;
        var mockDiscounts = new List<Discount>
        {
            new Discount { Id = 1, ProductID = productId, ProductName = "Discount 1" }
        };
        await _context.Discounts.AddRangeAsync(mockDiscounts);
        await _context.SaveChangesAsync();

        var result = await _discountService.GetDiscountByProductIdAsync(productId);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(productId, result.First().ProductID);
    }
}
