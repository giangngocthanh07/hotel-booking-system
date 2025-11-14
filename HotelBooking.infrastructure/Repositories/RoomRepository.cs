using HotelBooking.infrastructure.Models;

public interface IRoomRepository : IRepository<Room> { }
public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(HotelBookingDBContext context) : base(context) { }
}
