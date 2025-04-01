using Homework.Models;

public interface IProductService
{
    Task<Product> CreateProductAsync(Product product);
    Task<List<Product>> GetProductsAsync(string name);
    Task<Product> GetProductByIdAsync(int id);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
}
