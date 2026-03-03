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
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<(List<T> Items, int TotalCount)> GetPagedAsync(
    Expression<Func<T, bool>> filter,
    int pageIndex,
    int pageSize,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
}

public class Repository<T> : IRepository<T> where T : class     // Dependency Inversion applied here
{
    protected readonly HotelBookingDBContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(HotelBookingDBContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task AddAsync(T entity)
    {
        // Check if the entity has an "Id" property of type int
        var prop = typeof(T).GetProperty("Id");
        if (prop != null && prop.PropertyType == typeof(int))
        {
            // Reset to 0 to prevent manual ID injection
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

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().AnyAsync(predicate);

    public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
    Expression<Func<T, bool>> filter,
    int pageIndex,
    int pageSize,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        // 1. Build query with filter
        var query = _dbSet.AsNoTracking().Where(filter);

        // 2. Count total records (IMPORTANT: count before slicing)
        int totalCount = await query.CountAsync();

        // 3. Apply ordering (required before Skip/Take)
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            // EF.Property allows accessing properties by string name
            // Note: Type must match exactly (int); will throw if Id is Guid or string

            // ISSUE: SQL Server requires ORDER BY when using Skip/Take.
            // Without it, Skip may fail or return inconsistent results.

            // If the caller doesn't care about ordering, pass orderBy: null
            // _repo.GetPagedAsync(..., orderBy: null);
            // SOLUTION: Repository defaults to ordering by "Id" descending.
            query = query.OrderByDescending(x => EF.Property<int>(x, "Id"));
        }

        // 4. Apply pagination
        var items = await query
            .Skip((pageIndex - 1) * pageSize) // Skip previous pages
            .Take(pageSize)                   // Take the requested page size
            .ToListAsync();

        return (items, totalCount);
    }
}

