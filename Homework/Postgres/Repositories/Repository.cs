using Homework.DBEntities;
using Homework.Models;
using Microsoft.EntityFrameworkCore;

public class PostgreRepostiory<T> : IPostgreRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;

    public PostgreRepostiory(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<T>> GetAllItemsAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> GetItemByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T> AddItemAsync(T model)
    {
        await _context.Set<T>().AddAsync(model);
        await _context.SaveChangesAsync();
        return model;
    }

    public async Task<T> UpdateItemAsync(T model)
    {
        _context.Set<T>().Update(model);
        await _context.SaveChangesAsync();
        return model;
    }

    public async Task DeleteItemAsync(int id)
    {
        var item = await _context.Set<T>().FindAsync(id);
        if (item != null)
        {
            _context.Set<T>().Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Product> GetItemByStringAsync(string str)
    {
        return await _context.Set<Product>()
            .Where(item => EF.Functions.Like(item.Name, $"%{str}%"))
            .FirstOrDefaultAsync();
    }
}
