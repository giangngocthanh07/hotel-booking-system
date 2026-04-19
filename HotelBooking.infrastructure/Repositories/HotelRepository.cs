using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<List<SearchHotelResult>> GetSearchHotelsAsync(string cityName, DateTime? checkIn, DateTime? checkOut, int? adults, int? children, int? rooms);

    Task<(List<Hotel> Items, int TotalCount)> GetPagedWithUserAsync(
        Expression<Func<Hotel, bool>>? filter,
        int pageIndex,
        int pageSize);

    Task<IEnumerable<Hotel>> GetAllPendingRequestsAsync();
    Task<Hotel?> GetByIdWithOwnerAsync(int id);
    // Get distinct request statuses for filtering options
    Task<List<string>> GetDistinctStatusesAsync();
}
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelBookingDBContext context) : base(context)
    {
    }

    public async Task<List<SearchHotelResult>> GetSearchHotelsAsync(string cityName, DateTime? checkIn, DateTime? checkOut, int? adults, int? children, int? rooms)
    {
        var results = await _context.Set<SearchHotelResult>()
            .FromSqlInterpolated($@"
                EXEC sp_SearchHotels 
                    @CityName={cityName}, 
                    @CheckIn={checkIn}, 
                    @CheckOut={checkOut}, 
                    @Adults={adults}, 
                    @Children={children}, 
                    @Rooms={rooms}")
            .ToListAsync();

        return results;
    }

    public async Task<(List<Hotel> Items, int TotalCount)> GetPagedWithUserAsync(
        Expression<Func<Hotel, bool>>? filter,
        int pageIndex,
        int pageSize)
    {
        // 1. Base query with Include User
        var query = _dbSet.AsNoTracking().Include(ur => ur.Owner).AsQueryable();

        // 2. Apply filter if provided
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // 3. Count total records (before pagination)
        int totalCount = await query.CountAsync();

        // 4. Sort by most recent CreatedAt + paginate
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Hotel>> GetAllPendingRequestsAsync()
    {
        return await _dbSet
        .Include(h => h.Owner)
        .Where(h => h.Status == "Pending").ToListAsync();
    }

    public async Task<Hotel?> GetByIdWithOwnerAsync(int id)
    {
        return await _dbSet.AsNoTracking()
            .Include(h => h.Owner)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<List<string>> GetDistinctStatusesAsync()
    {
        return await _dbSet.AsNoTracking()
                           .Where(s => s != null)
                           .Select(r => r.Status!)
                           .Distinct()
                           .ToListAsync();
    }
}
