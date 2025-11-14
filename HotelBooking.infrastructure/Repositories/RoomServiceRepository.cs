using HotelBooking.infrastructure.Models;

public interface IRoomServiceRepository : IRepository<RoomService> { }
public class RoomServiceRepository : Repository<RoomService>, IRoomServiceRepository
{
    public RoomServiceRepository(HotelBookingDBContext context) : base(context) { }
}
