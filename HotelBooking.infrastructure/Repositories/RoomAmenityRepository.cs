using HotelBooking.infrastructure.Models;

public interface IRoomAmenityRepository : IRepository<RoomAmenity> { }

public class RoomAmenityRepository : Repository<RoomAmenity>, IRoomAmenityRepository
{
    public RoomAmenityRepository(HotelBookingDBContext context) : base(context) { }
}