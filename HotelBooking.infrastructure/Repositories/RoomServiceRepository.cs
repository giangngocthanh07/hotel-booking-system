using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomServiceRepository : IRepository<RoomService> { }
public class RoomServiceRepository : Repository<RoomService>, IRoomServiceRepository
{
    public RoomServiceRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
