using HotelBooking.infrastructure.Models;

public interface IRoomQualityGroupRepository : IRepository<RoomQualityGroup>
{
    // Add custom methods for RoomQualityGroup here if needed
}

public class RoomQualityGroupRepository : Repository<RoomQualityGroup>, IRoomQualityGroupRepository
{
    public RoomQualityGroupRepository(HotelBookingDBContext context) : base(context)
    {
    }
}