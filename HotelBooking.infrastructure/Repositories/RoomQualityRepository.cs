using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomQualityRepository : IRepository<RoomQuality>
{
    // Add custom methods for RoomQuality here if needed
}

public class RoomQualityRepository : Repository<RoomQuality>, IRoomQualityRepository
{
    public RoomQualityRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}