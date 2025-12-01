using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IPolicyRepository : IRepository<Policy>
{
    Task<IEnumerable<Policy>> GetPoliciesByTypeAsync(int? typeId);
}
public class PolicyRepository : Repository<Policy>, IPolicyRepository
{
    public PolicyRepository(HotelBookingDBContext context) : base(context) { }

    public async Task<IEnumerable<Policy>> GetPoliciesByTypeAsync(int? typeId)
    {
        var query = _dbSet.Include(p => p.PolicyType).AsQueryable();

        if (typeId.HasValue)
        {
            query = query.Where(p => p.PolicyTypeId == typeId);
        }

        return await query.ToListAsync();
    }
}
