using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Data;

namespace WorkoutGamifier.Core.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly TestDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(TestDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> CreateAsync(T entity)
    {
        _dbSet.Add(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}