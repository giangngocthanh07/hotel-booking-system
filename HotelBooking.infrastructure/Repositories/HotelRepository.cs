using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<List<SearchHotelResult>> GetSearchHotelsAsync(string cityName, DateTime? checkIn, DateTime? checkOut, int? adults, int? children, int? rooms);
    Task<Hotel?> GetHotelDetailsByIdAsync(int id);
}
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }

    public async Task<Hotel?> GetHotelDetailsByIdAsync(int id)
    {
        // FR-ONB-016 / FR-CAT-006/007: public links must respect approval and operational state.
        // Keep this local to public details so management reads can still access hidden Hotels.
        return await _dbSet
            .Where(h => h.Id == id && h.IsVerified == true && h.Status == "Active" && h.IsDeleted != true)
            .Include(h => h.HotelImages)
            .Include(h => h.HotelAmenities).ThenInclude(ha => ha.Amenity)
            .Include(h => h.RoomTypes).ThenInclude(rt => rt.RoomImages)
            .Include(h => h.RoomTypes).ThenInclude(rt => rt.RoomAmenities).ThenInclude(ra => ra.Amenity)
            .Include(h => h.Reviews).ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(_cancellationToken);
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
