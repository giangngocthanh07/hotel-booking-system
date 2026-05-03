using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomTypeRepository : IRepository<RoomType> { }
public class RoomTypeRepository : Repository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
