using HotelBooking.infrastructure.Models;

public interface IHotelImageRepository : IRepository<HotelImage>
{
    // Add custom methods for HotelImage here if needed
}

public class HotelImageRepository : Repository<HotelImage>, IHotelImageRepository
{
    public HotelImageRepository(HotelBookingDBContext context) : base(context)
    {
    }
}