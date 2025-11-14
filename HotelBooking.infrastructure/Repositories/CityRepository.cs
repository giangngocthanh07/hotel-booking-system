using HotelBooking.infrastructure.Models;

public interface ICityRepository : IRepository<City> { }
public class CityRepository : Repository<City>, ICityRepository
{
    public CityRepository(HotelBookingDBContext context) : base(context) { }
}




