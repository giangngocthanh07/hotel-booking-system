using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;

public interface IHotelApprovalRequestRepository : IRepository<HotelApprovalRequest>
{
    // Add custom methods for HotelApprovalRequest here if needed
    Task<(List<HotelApprovalRequest> Items, int TotalCount)> GetPagedWithUserAsync(
            Expression<Func<HotelApprovalRequest, bool>>? filter,
            int pageIndex,
            int pageSize);

    Task<IEnumerable<HotelApprovalRequest>> GetPendingByIdAsync(int ownerId);
    Task<IEnumerable<HotelApprovalRequest>> GetAllPendingRequestsAsync();
    Task<HotelApprovalRequest?> GetByIdWithOwnerAsync(int id);
    Task<List<HotelApprovalRequest>> GetByUserIdAsync(int userId);

    Task<List<string>> GetDistinctStatusesAsync();

    Task<List<HotelApprovalRequest>> GetRecentAsync(int count);

    // Cập nhật lại kiểu trả về cho phù hợp với trạng thái số nguyên
    Task<(int Total, int Pending, int Approved, int Rejected, int Cancelled, int Today, int ThisWeek, int ThisMonth)> GetStatsRawAsync();

}

public class HotelApprovalRequestRepository : Repository<HotelApprovalRequest>, IHotelApprovalRequestRepository
{
    public HotelApprovalRequestRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }

    public async Task<(List<HotelApprovalRequest> Items, int TotalCount)> GetPagedWithUserAsync(
            Expression<Func<HotelApprovalRequest, bool>>? filter,
            int pageIndex,
            int pageSize)
    {
        var query = _dbSet.IgnoreQueryFilters().AsNoTracking().Include(ur => ur.Owner).AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(_cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<HotelApprovalRequest>> GetPendingByIdAsync(int ownerId)
    {
        // Status == Pending
        return await _dbSet
                .IgnoreQueryFilters()
                .Include(ur => ur.Owner)
                .Where(ur => ur.OwnerId == ownerId && ur.Status == "Pending")
                .ToListAsync(_cancellationToken);
    }

    public async Task<IEnumerable<HotelApprovalRequest>> GetAllPendingRequestsAsync()
    {
        // Status == Pending
        return await _dbSet.IgnoreQueryFilters()
                .Include(h => h.Owner)
                .Where(h => h.Status == "Pending").ToListAsync(_cancellationToken);
    }

    public async Task<HotelApprovalRequest?> GetByIdWithOwnerAsync(int id)
    {
        return await _dbSet.AsNoTracking()
                    .IgnoreQueryFilters()
                    .Include(h => h.Owner)
                    .FirstOrDefaultAsync(h => h.Id == id, _cancellationToken);
    }

    public async Task<List<string>> GetDistinctStatusesAsync()
    {
        return await _dbSet.IgnoreQueryFilters().AsNoTracking()
                           .Where(s => s != null)
                           .Select(r => r.Status!)
                           .Distinct()
                           .ToListAsync(_cancellationToken);
    }

    public async Task<List<HotelApprovalRequest>> GetByUserIdAsync(int userId)
    {
        // ĐÃ BỎ Include(Province, Ward)
        return await _dbSet.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(h => h.Owner)
            .Where(h => h.OwnerId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(_cancellationToken);
    }

    public async Task<List<HotelApprovalRequest>> GetRecentAsync(int count)
    {
        return await _dbSet.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(h => h.Owner)
            .OrderByDescending(h => h.CreatedAt)
            .Take(count)
            .ToListAsync(_cancellationToken);
    }

    public async Task<(int Total, int Pending, int Approved, int Rejected, int Cancelled, int Today, int ThisWeek, int ThisMonth)> GetStatsRawAsync()
    {
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Tối ưu: Chỉ query những gì cần thiết thay vì lôi toàn bộ data về RAM
        var allRequests = await _dbSet.AsNoTracking()
                                      .Select(r => new { r.Status, r.CreatedAt })
                                      .ToListAsync(_cancellationToken);

        return (
            Total: allRequests.Count,
            Pending: allRequests.Count(r => r.Status == "Pending"),
            Approved: allRequests.Count(r => r.Status == "Approved"),
            Rejected: allRequests.Count(r => r.Status == "Rejected"),
            Cancelled: allRequests.Count(r => r.Status == "Cancelled"),
            Today: allRequests.Count(r => r.CreatedAt.Date == today),
            ThisWeek: allRequests.Count(r => r.CreatedAt >= weekStart),
            ThisMonth: allRequests.Count(r => r.CreatedAt >= monthStart)
        );
    }
}
