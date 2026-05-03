using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomQualityGroupRepository : IRepository<RoomQualityGroup>
{
    // Add custom methods for RoomQualityGroup here if needed
}

public class RoomQualityGroupRepository : Repository<RoomQualityGroup>, IRoomQualityGroupRepository
{
    public RoomQualityGroupRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}