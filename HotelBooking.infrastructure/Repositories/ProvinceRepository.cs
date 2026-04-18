using HotelBooking.infrastructure.Models;

public interface IProvinceRepository : IRepository<Province>
{
    // Add custom methods for Province here if needed
}

public class ProvinceRepository : Repository<Province>, IProvinceRepository
{
    public ProvinceRepository(HotelBookingDBContext context) : base(context)
    {
    }
}