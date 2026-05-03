using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IHotelAmenityRepository : IRepository<HotelAmenity>
{
    // Add custom methods for HotelAmenity here if needed
}

public class HotelAmenityRepository : Repository<HotelAmenity>, IHotelAmenityRepository
{
    public HotelAmenityRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}