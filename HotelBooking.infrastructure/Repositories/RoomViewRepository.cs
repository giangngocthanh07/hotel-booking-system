using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomViewRepository : IRepository<RoomView>
{
    // Add custom methods for RoomView here if needed
}

public class RoomViewRepository : Repository<RoomView>, IRoomViewRepository
{
    public RoomViewRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}