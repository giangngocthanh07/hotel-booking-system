using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<List<SearchHotelResult>> GetSearchHotelsAsync(string cityName, DateTime? checkIn, DateTime? checkOut, int? adults, int? children, int? rooms);
}
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
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
            .ToListAsync(_cancellationToken);

        return results;
    }
}
