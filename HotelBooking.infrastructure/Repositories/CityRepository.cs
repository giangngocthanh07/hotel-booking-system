using HotelBooking.infrastructure.Models;

public interface ICityRepository : IRepository<City> { }
public class CityRepository : Repository<City>, ICityRepository
{
    public CityRepository(HotelBookingContext context) : base(context) { }
}




