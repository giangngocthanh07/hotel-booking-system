using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


public interface IUpgradeRequestRepository : IRepository<UpgradeRequest>
{
    // Add custom methods for UpgradeRequest here if needed
    Task<IEnumerable<UpgradeRequest>> GetPendingByIdAsync(int id);
    Task<IEnumerable<UpgradeRequest>> GetAllPendingRequestsAsync();

    /// <summary>
    /// Get paged list of upgrade requests with user information, with optional filtering
    /// </summary>
    Task<(List<UpgradeRequest> Items, int TotalCount)> GetPagedWithUserAsync(
        Expression<Func<UpgradeRequest, bool>>? filter,
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Get upgrade request by ID with user information
    /// </summary>
    Task<UpgradeRequest?> GetByIdWithUserAsync(int id);

    /// <summary>
    /// Get statistics of requests by status and time periods
    /// </summary>
    Task<(int Total, int Pending, int Approved, int Rejected, int Cancelled, int Today, int ThisWeek, int ThisMonth)> GetStatsRawAsync();

    /// <summary>
    /// Get recent upgrade requests with user information, ordered by request date descending
    /// </summary>
    Task<List<UpgradeRequest>> GetRecentAsync(int count);

    /// <summary>
    /// Get all upgrade requests for a specific user (including processed ones)
    /// </summary>
    Task<List<UpgradeRequest>> GetByUserIdAsync(int userId);

    // Get distinct request statuses for filtering options
    Task<List<string>?> GetDistinctStatusesAsync();


}

public class UpgradeRequestRepository : Repository<UpgradeRequest>, IUpgradeRequestRepository
{
    public UpgradeRequestRepository(HotelBookingDBContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UpgradeRequest>> GetPendingByIdAsync(int id)
    {
        var requests = await _dbSet.Include(ur => ur.User)
                                  .Where(ur => ur.UserId == id && ur.Status == "Pending")
                                  .ToListAsync();
        return requests;
    }

    public async Task<IEnumerable<UpgradeRequest>> GetAllPendingRequestsAsync()
    {
        return await _dbSet.Include(ur => ur.User)
                           .Where(ur => ur.Status == "Pending")
                           .ToListAsync();
    }

    public async Task<(List<UpgradeRequest> Items, int TotalCount)> GetPagedWithUserAsync(
        Expression<Func<UpgradeRequest, bool>>? filter,
        int pageIndex,
        int pageSize)
    {
        // 1. Base query with Include User
        var query = _dbSet.AsNoTracking().Include(ur => ur.User).AsQueryable();

        // 2. Apply filter if provided
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // 3. Count total records (before pagination)
        int totalCount = await query.CountAsync();

        // 4. Sort by most recent RequestedAt + paginate
        var items = await query
            .OrderByDescending(r => r.RequestedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<UpgradeRequest?> GetByIdWithUserAsync(int id)
    {
        return await _dbSet.AsNoTracking().Include(ur => ur.User).FirstOrDefaultAsync();
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
            Today: allRequests.Count(r => r.RequestedAt.Date == today),
            ThisWeek: allRequests.Count(r => r.RequestedAt >= weekStart),
            ThisMonth: allRequests.Count(r => r.RequestedAt >= monthStart)
        );
    }

    public async Task<List<UpgradeRequest>> GetRecentAsync(int count)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.User)
            .OrderByDescending(r => r.RequestedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<string>?> GetDistinctStatusesAsync()
    {
        return await _dbSet.AsNoTracking()
                           .Select(r => r.Status)
                           .Where(s => s != null)
                           .Distinct()
                           .ToListAsync();
    }

    public async Task<List<UpgradeRequest>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();
    }
}