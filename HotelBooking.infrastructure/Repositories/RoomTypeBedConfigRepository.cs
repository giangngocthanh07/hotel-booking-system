using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomTypeBedConfigRepository : IRepository<RoomTypeBedConfig>
{
    // Add custom methods for RoomTypeBedConfig here if needed
}

public class RoomTypeBedConfigRepository : Repository<RoomTypeBedConfig>, IRoomTypeBedConfigRepository
{
    public RoomTypeBedConfigRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}