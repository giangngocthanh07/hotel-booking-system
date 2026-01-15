using HotelBooking.infrastructure.Models;

public interface IRoomViewRepository : IRepository<RoomView>
{
    // Add custom methods for RoomView here if needed
}

public class RoomViewRepository : Repository<RoomView>, IRoomViewRepository
{
    public RoomViewRepository(HotelBookingDBContext context) : base(context)
    {
    }
}