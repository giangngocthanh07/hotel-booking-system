using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly HotelBookingDBContext _HBcontext;
    protected readonly DbSet<T> _dbSet;

    public Repository(HotelBookingDBContext HBcontext)
    {
        _HBcontext = HBcontext;
        _dbSet = _HBcontext.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task AddAsync(T entity)
    {
        // Kiểm tra nếu có property "Id" và kiểu là int
        var prop = typeof(T).GetProperty("Id");
        if (prop != null && prop.PropertyType == typeof(int))
        {
            // Reset về 0 để tránh insert thủ công
            prop.SetValue(entity, 0);
        }

        await _dbSet.AddAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().SingleOrDefaultAsync(predicate);


    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().AnyAsync(predicate);
}

