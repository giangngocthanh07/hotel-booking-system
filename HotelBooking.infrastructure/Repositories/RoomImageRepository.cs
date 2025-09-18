using HotelBooking.infrastructure.Models;

public interface IRoomImageRepository : IRepository<RoomImage> { }
public class RoomImageRepository : Repository<RoomImage>, IRoomImageRepository
{
    public RoomImageRepository(HotelBookingContext context) : base(context) { }
}
