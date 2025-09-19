using HotelBooking.infrastructure.Models;

public interface IAmenityRepository : IRepository<Amenity> { }
public class AmenityRepository : Repository<Amenity>, IAmenityRepository
{
    public AmenityRepository(HotelBookingContext context) : base(context) { }
}
