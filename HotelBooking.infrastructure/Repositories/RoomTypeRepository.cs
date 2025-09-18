using HotelBooking.infrastructure.Models;

public interface IRoomTypeRepository : IRepository<RoomType> { }
public class RoomTypeRepository : Repository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(HotelBookingContext context) : base(context) { }
}
