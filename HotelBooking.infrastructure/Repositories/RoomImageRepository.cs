using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomImageRepository : IRepository<RoomImage> { }
public class RoomImageRepository : Repository<RoomImage>, IRoomImageRepository
{
    public RoomImageRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
