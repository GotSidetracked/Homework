using Homework.DTOs.Discount;
using Homework.Models;

public interface IDiscountService
{
    Task<Discount> CreateDiscountAsync(AddDiscount discount);
    Task<List<Discount>> GetDiscountsAsync();
    Task<Discount> GetDiscountByIdAsync(int id);
    Task<List<Discount>> GetDiscountByProductIdAsync(int id);
    Task<bool> UpdateDiscountAsync(Discount discount);
    Task<bool> DeleteDiscountAsync(int id);
}
