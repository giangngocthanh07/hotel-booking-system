using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;


public interface IUpgradeRequestRepository : IRepository<UpgradeRequest>
{
    // Add custom methods for UpgradeRequest here if needed
    Task<IEnumerable<UpgradeRequest>> GetPendingByIdAsync(int id);
    Task<IEnumerable<UpgradeRequest>> GetAllPendingRequestsAsync();

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

}