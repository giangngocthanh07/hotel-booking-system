using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;

public interface IHotelImageRepository : IRepository<HotelImage>
{
    Task<Hotel?> GetOwnedGalleryAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default);
}

public class HotelImageRepository : Repository<HotelImage>, IHotelImageRepository
{
    public async Task<Hotel?> GetOwnedGalleryAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default) =>
        // FR-CAT-003 / FR-MED-005: management includes Inactive/Suspended Hotels without exposing another Owner's gallery.
        await _context.Hotels.AsNoTracking().Where(hotel => hotel.Id == hotelId && hotel.OwnerId == ownerId && hotel.IsDeleted != true)
            .Include(hotel => hotel.HotelImages.Where(image => image.IsDeleted != true)
                .OrderBy(image => image.DisplayOrder).ThenBy(image => image.Id))
            .SingleOrDefaultAsync(cancellationToken);

    public HotelImageRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}
