using Homework.Models;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class InvoiceServiceTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IDiscountService> _mockDiscountService;
    private readonly InvoiceService _invoiceService;

    public InvoiceServiceTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _mockCache = new Mock<IMemoryCache>();
        _mockDiscountService = new Mock<IDiscountService>();

        _invoiceService = new InvoiceService(_mockContext.Object, _mockCache.Object, new ConfigurationBuilder().Build(), _mockDiscountService.Object);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldCreateInvoiceWithCorrectDiscount()
    {
        var product = new Product { Id = 1, Name = "Product 1", Price = 100 };
        var invoice = new Invoice { Id = 1, ProductId = 1, Quantity = 2, Price = 0 };

        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);
        _mockDiscountService.Setup(d => d.GetDiscountByProductIdAsync(1)).ReturnsAsync(new List<Discount>
        {
            new Discount { ProductID = 1, ProductName = "Produce", Id = 1, DiscountPercentile = 10, DiscountFlat = null, IsFlat = false }
        });

        var result = await _invoiceService.CreateInvoiceAsync(invoice);

        Assert.Equal(180, result.Price); 
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_ShouldReturnInvoice()
    {
        var invoice = new Invoice { Id = 1, ProductId = 1, Quantity = 2, Price = 200 };
        _mockContext.Setup(c => c.Invoices.FindAsync(1)).ReturnsAsync(invoice);

        var result = await _invoiceService.GetInvoiceByIdAsync(1);

        Assert.Equal(invoice, result);
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldReturnListOfInvoices()
    {
        var invoices = new List<Invoice>
        {
            new Invoice { Id = 1, ProductId = 1, Quantity = 2, Price = 200 },
            new Invoice { Id = 2, ProductId = 2, Quantity = 1, Price = 150 }
        };

        _mockContext.Setup(c => c.Invoices.ToListAsync(default)).ReturnsAsync(invoices);

        var result = await _invoiceService.GetInvoicesAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_ShouldUpdateInvoice()
    {
        var existingInvoice = new Invoice { Id = 1, ProductId = 1, Quantity = 2, Price = 200 };
        _mockContext.Setup(c => c.Invoices.FindAsync(1)).ReturnsAsync(existingInvoice);

        existingInvoice.Quantity = 3; 

        var result = await _invoiceService.UpdateInvoiceAsync(existingInvoice);

        Assert.True(result);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task DeleteInvoiceAsync_ShouldDeleteInvoice()
    {
        var invoice = new Invoice { Id = 1, ProductId = 1, Quantity = 2, Price = 200 };
        _mockContext.Setup(c => c.Invoices.FindAsync(1)).ReturnsAsync(invoice);

        var result = await _invoiceService.DeleteInvoiceAsync(1);
     
        Assert.True(result);
        _mockContext.Verify(c => c.SaveChanges(), Times.Once);
    }
}
