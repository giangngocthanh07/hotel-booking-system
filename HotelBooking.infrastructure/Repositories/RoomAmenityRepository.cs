using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoomAmenityRepository : IRepository<RoomAmenity> { }

public class RoomAmenityRepository : Repository<RoomAmenity>, IRoomAmenityRepository
{
    public RoomAmenityRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}