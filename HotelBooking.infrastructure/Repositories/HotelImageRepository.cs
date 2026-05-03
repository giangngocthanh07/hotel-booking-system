using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IHotelImageRepository : IRepository<HotelImage>
{
    // Add custom methods for HotelImage here if needed
}

public class HotelImageRepository : Repository<HotelImage>, IHotelImageRepository
{
    public HotelImageRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}