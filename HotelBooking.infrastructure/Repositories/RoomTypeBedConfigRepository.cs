using HotelBooking.infrastructure.Models;

public interface IRoomTypeBedConfigRepository : IRepository<RoomTypeBedConfig>
{
    // Add custom methods for RoomTypeBedConfig here if needed
}

public class RoomTypeBedConfigRepository : Repository<RoomTypeBedConfig>, IRoomTypeBedConfigRepository
{
    public RoomTypeBedConfigRepository(HotelBookingDBContext context) : base(context)
    {
    }
}