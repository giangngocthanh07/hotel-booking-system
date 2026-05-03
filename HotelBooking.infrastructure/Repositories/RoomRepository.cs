using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomRepository : IRepository<Room> { }
public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
