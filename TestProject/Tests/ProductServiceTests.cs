using Homework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

public class ProductServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _mockCache = new Mock<IMemoryCache>();
        _mockConfig = new Mock<IConfiguration>();
        _productService = new ProductService(_mockContext.Object, _mockCache.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnProduct_WhenProductIsValid()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 10.0m
        };

        _mockContext.Setup(c => c.Products.AddAsync(It.IsAny<Product>(), default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Product>)null);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _productService.CreateProductAsync(product);

        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(10.0m, result.Price);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 10.0m
        };

        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        var result = await _productService.GetProductByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnTrue_WhenProductIsUpdated()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 10.0m
        };

        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _productService.UpdateProductAsync(product);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnTrue_WhenProductIsDeleted()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 10.0m
        };

        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _productService.DeleteProductAsync(1);

        Assert.True(result);
    }
}
