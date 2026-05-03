using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;

public interface IWardRepository : IRepository<Ward>
{
    // Add custom methods for Ward here if needed
    Task<IEnumerable<Ward>> GetByProvinceIdAsync(int provinceId);
}

public class WardRepository : Repository<Ward>, IWardRepository
{
    public WardRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }

    public async Task<IEnumerable<Ward>> GetByProvinceIdAsync(int provinceId)
    {
        return await _dbSet.Where(x => x.ProvinceId == provinceId).ToListAsync(_cancellationToken);
    }
}