using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<(List<Hotel> Items, int TotalCount)> GetPageByOwnerAsync(int ownerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Hotel?> GetByOwnerAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default);
    Task<List<SearchHotelResult>> GetSearchHotelsAsync(string cityName, DateTime? checkIn, DateTime? checkOut, int? adults, int? children, int? rooms);
    Task<Hotel?> GetHotelDetailsByIdAsync(int id);
}
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public async Task<(List<Hotel> Items, int TotalCount)> GetPageByOwnerAsync(int ownerId, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default)
    {
        // FR-CAT-001/003/008: include Inactive/Suspended Hotels, scoped before count and pagination.
        var query = _dbSet.AsNoTracking().Where(hotel => hotel.OwnerId == ownerId && hotel.IsDeleted != true);
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(hotel => hotel.CreatedAt).ThenByDescending(hotel => hotel.Id)
            .Skip(checked((pageIndex - 1) * pageSize)).Take(pageSize).ToListAsync(cancellationToken);
        return (items, count);
    }

    public async Task<Hotel?> GetByOwnerAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default) =>
        // FR-CAT-003: foreign and deleted IDs share the same unavailable result.
        await _dbSet.AsNoTracking().Where(hotel => hotel.OwnerId == ownerId && hotel.Id == hotelId && hotel.IsDeleted != true)
            .Include(hotel => hotel.Country).Include(hotel => hotel.Province)
            .Include(hotel => hotel.Ward).Include(hotel => hotel.PropertyType)
            .SingleOrDefaultAsync(cancellationToken);

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
