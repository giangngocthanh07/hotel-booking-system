using HotelBooking.infrastructure.Models;

public interface IAmenityRepository : IRepository<Amenity> { }
public class AmenityRepository : Repository<Amenity>, IAmenityRepository
{
    public AmenityRepository(HotelBookingDBContext context) : base(context) { }
}
