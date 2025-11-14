using HotelBooking.infrastructure.Models;

public interface ICountryRepository : IRepository<Country> { }
public class CountryRepository : Repository<Country>, ICountryRepository
{
    public CountryRepository(HotelBookingDBContext context) : base(context) { }
}
