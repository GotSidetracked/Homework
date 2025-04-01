using Homework.DBEntities;
using Homework.Models;

public interface IPostgreRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllItemsAsync();
    Task<T> GetItemByIdAsync(int id);
    Task<T> AddItemAsync(T model);
    Task<T> UpdateItemAsync(T model);
    Task DeleteItemAsync(int id);
    Task<Product> GetItemByStringAsync(string str);
}
