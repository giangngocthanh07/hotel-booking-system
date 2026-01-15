using HotelBooking.infrastructure.Models;

public interface IRoomQualityRepository : IRepository<RoomQuality>
{
    // Add custom methods for RoomQuality here if needed
}

public class RoomQualityRepository : Repository<RoomQuality>, IRoomQualityRepository
{
    public RoomQualityRepository(HotelBookingDBContext context) : base(context)
    {
    }
}