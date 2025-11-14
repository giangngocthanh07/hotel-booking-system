using HotelBooking.infrastructure.Models;

public interface IRoomTypeRepository : IRepository<RoomType> { }
public class RoomTypeRepository : Repository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(HotelBookingDBContext context) : base(context) { }
}
