using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


public interface IUpgradeRequestRepository : IRepository<UpgradeRequest>
{
    // Add custom methods for UpgradeRequest here if needed
    Task<IEnumerable<UpgradeRequest>> GetPendingByIdAsync(int id);
    Task<IEnumerable<UpgradeRequest>> GetAllPendingRequestsAsync();
    
    /// <summary>
    /// Lấy danh sách Request có phân trang, Include User
    /// </summary>
    Task<(List<UpgradeRequest> Items, int TotalCount)> GetPagedWithUserAsync(
        Expression<Func<UpgradeRequest, bool>>? filter,
        int pageIndex,
        int pageSize);
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
        // 1. Query base với Include User
        var query = _dbSet.AsNoTracking().Include(ur => ur.User).AsQueryable();

        // 2. Apply filter nếu có
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // 3. Đếm tổng số bản ghi (trước khi phân trang)
        int totalCount = await query.CountAsync();

        // 4. Sắp xếp theo RequestedAt mới nhất + phân trang
        var items = await query
            .OrderByDescending(r => r.RequestedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}