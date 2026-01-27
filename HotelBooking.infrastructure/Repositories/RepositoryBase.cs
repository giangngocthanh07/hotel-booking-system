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

public class Repository<T> : IRepository<T> where T : class     // Đảo ngượv sự phụ thuộc ở đây
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

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AsNoTracking().AnyAsync(predicate);

    public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
    Expression<Func<T, bool>> filter,
    int pageIndex,
    int pageSize,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        // 1. Tạo query và lọc
        var query = _dbSet.AsNoTracking().Where(filter);

        // 2. Đếm tổng số bản ghi (QUAN TRỌNG: Đếm trước khi cắt trang)
        int totalCount = await query.CountAsync();

        // 3. Sắp xếp (nếu có) - Bắt buộc phải sort trước khi Skip/Take
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            // EF.Property giúp truy cập thuộc tính bằng chuỗi string
            // Lưu ý: Phải đúng kiểu dữ liệu (int), nếu Id là Guid hay String thì sẽ lỗi

            // VẤN ĐỀ: Để phân trang (Skip/Take), SQL Server BẮT BUỘC phải có ORDER BY.
            // Nếu không có Order By, lệnh Skip sẽ bị lỗi hoặc kết quả lung tung.

            // Nếu bên đối tượng (ví dụ ServiceManager) không quan tâm sắp xếp, để null và không sắp xếp mà code như dưới
            // _repo.GetPagedAsync(..., orderBy: null);
            // GIẢI PHÁP: Repository tự chữa cháy bằng cách sắp xếp theo cột "Id" mặc định như code bên dưới.
            query = query.OrderByDescending(x => EF.Property<int>(x, "Id"));
        }

        // 4. Cắt trang (Pagination Logic)
        var items = await query
            .Skip((pageIndex - 1) * pageSize) // Bỏ qua các trang trước
            .Take(pageSize)                   // Lấy số lượng cần thiết
            .ToListAsync();

        return (items, totalCount);
    }
}

