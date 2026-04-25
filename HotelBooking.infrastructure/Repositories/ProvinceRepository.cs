using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IProvinceRepository : IRepository<Province>
{
    // Add custom methods for Province here if needed
    Task<IEnumerable<Province>> GetByCountryIdAsync(int countryId);
}

public class ProvinceRepository : Repository<Province>, IProvinceRepository
{
    public ProvinceRepository(HotelBookingDBContext context) : base(context)
    {


    }

    public async Task<IEnumerable<Province>> GetByCountryIdAsync(int countryId)
    {
        return await _dbSet.Where(x => x.CountryId == countryId).ToListAsync();
    }
}
