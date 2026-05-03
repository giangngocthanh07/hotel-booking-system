using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface ICountryRepository : IRepository<Country>
{
}
public class CountryRepository : Repository<Country>, ICountryRepository
{
    public CountryRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {

    }


}
