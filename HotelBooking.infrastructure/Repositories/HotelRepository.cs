using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<List<SearchHotelResult>> GetSearchHotelsAsync(string cityName, DateTime? checkIn, DateTime? checkOut, int? adults, int? children, int? rooms);


}
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    private readonly HotelBookingDBContext _context;
    public HotelRepository(HotelBookingDBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<SearchHotelResult>> GetSearchHotelsAsync(string cityName, DateTime? checkIn, DateTime? checkOut, int? adults, int? children, int? rooms)
    {
        var results = await _context.Set<SearchHotelResult>()
            .FromSqlInterpolated($@"
                EXEC sp_SearchHotels 
                    @CityName={cityName}, 
                    @CheckIn={checkIn}, 
                    @CheckOut={checkOut}, 
                    @Adults={adults}, 
                    @Children={children}, 
                    @Rooms={rooms}")
            .ToListAsync();

        return results;
    }
}
