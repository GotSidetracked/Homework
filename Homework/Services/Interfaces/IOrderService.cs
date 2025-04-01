using Homework.Models;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order order);
    Task<List<Order>> GetOrdersAsync();
    Task<Order> GetOrderByIdAsync(int id);
    Task<bool> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(int id);
}
