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

    Task<IEnumerable<Hotel>> GetPendingByIdAsync(int id);
    Task<IEnumerable<Hotel>> GetAllPendingRequestsAsync();
    Task<Hotel?> GetByIdWithOwnerAsync(int id);
    Task<List<Hotel>> GetByUserIdAsync(int userId);
    // Get distinct request statuses for filtering options
    Task<List<string>> GetDistinctStatusesAsync();
    /// <summary>
    /// Get recent upgrade requests with user information, ordered by request date descending
    /// </summary>
    Task<List<Hotel>> GetRecentAsync(int count);
    /// <summary>
    /// Get statistics of requests by status and time periods
    /// </summary>
    Task<(int Total, int Pending, int Approved, int Rejected, int Cancelled, int Today, int ThisWeek, int ThisMonth)> GetStatsRawAsync();
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

    public async Task<IEnumerable<Hotel>> GetPendingByIdAsync(int id)
    {
        var request = await _dbSet
                .IgnoreQueryFilters()
                .Include(ur => ur.Owner)
                .Where(ur => ur.OwnerId == id && ur.Status == "Pending").ToListAsync();
        return request;
    }

    public async Task<(List<Hotel> Items, int TotalCount)> GetPagedWithUserAsync(
        Expression<Func<Hotel, bool>>? filter,
        int pageIndex,
        int pageSize)
    {
        // 1. Base query with Include User
        var query = _dbSet.IgnoreQueryFilters().AsNoTracking().Include(ur => ur.Owner).AsQueryable();

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
        return await _dbSet.IgnoreQueryFilters()
        .Include(h => h.Owner)
        .Where(h => h.Status == "Pending").ToListAsync();
    }

    public async Task<Hotel?> GetByIdWithOwnerAsync(int id)
    {
        return await _dbSet.AsNoTracking()
                    .IgnoreQueryFilters()
                       .Include(h => h.Owner)
                       .Include(h => h.Province)
                       .Include(h => h.Ward)
                       .Include(h => h.Country)
                       .Include(h => h.PropertyType)
                       .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<List<string>> GetDistinctStatusesAsync()
    {
        return await _dbSet.IgnoreQueryFilters().AsNoTracking()
                           .Where(s => s != null)
                           .Select(r => r.Status!)
                           .Distinct()
                           .ToListAsync();
    }

    public async Task<List<Hotel>> GetByUserIdAsync(int userId)
    {
        return await _dbSet.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(h => h.Owner)
            .Where(h => h.OwnerId == userId)
            .Include(h => h.Province)
            .Include(h => h.Ward)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Hotel>> GetRecentAsync(int count)
    {
        var validStatuses = new[] { "Pending", "Approved", "Rejected", "Cancelled" };

        return await _dbSet.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(h => h.Owner)
            .Where(h => validStatuses.Contains(h.Status))
            .OrderByDescending(h => h.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(int Total, int Pending, int Approved, int Rejected, int Cancelled, int Today, int ThisWeek, int ThisMonth)> GetStatsRawAsync()
    {
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var allRequests = await _dbSet.AsNoTracking().ToListAsync();

        return (
            Total: allRequests.Count,
            Pending: allRequests.Count(r => r.Status == "Pending"),
            Approved: allRequests.Count(r => r.Status == "Approved"),
            Rejected: allRequests.Count(r => r.Status == "Rejected"),
            Cancelled: allRequests.Count(r => r.Status == "Cancelled"),
            Today: allRequests.Count(r => r.CreatedAt?.Date == today),
            ThisWeek: allRequests.Count(r => r.CreatedAt >= weekStart),
            ThisMonth: allRequests.Count(r => r.CreatedAt >= monthStart)
        );
    }
}
