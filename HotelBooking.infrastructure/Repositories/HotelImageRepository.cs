using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;

public interface IHotelImageRepository : IRepository<HotelImage>
{
    Task<Hotel?> GetOwnedGalleryAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default);
    Task<bool> OwnsHotelAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default);
    Task<Hotel?> GetOwnedForMediaWriteAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default);
    Task<int?> GetMaximumDisplayOrderAsync(int hotelId, CancellationToken cancellationToken = default);
    Task<int> GetAdditionalImageCountAsync(int hotelId, string? coverUrl, CancellationToken cancellationToken = default);
    Task AddGalleryImageAsync(HotelImage image, CancellationToken cancellationToken = default);
}

public class HotelImageRepository : Repository<HotelImage>, IHotelImageRepository
{
    public async Task<bool> OwnsHotelAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default) =>
        await _context.Hotels.AnyAsync(hotel => hotel.Id == hotelId && hotel.OwnerId == ownerId && hotel.IsDeleted != true, cancellationToken);

    public async Task<Hotel?> GetOwnedForMediaWriteAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction == null) throw new InvalidOperationException("Hotel media writes require a transaction.");
        // FR-MED-005 / FR-CAT-006: serialize media writes per Hotel before reading positions or readiness counts.
        return await _context.Hotels.FromSqlInterpolated($@"SELECT * FROM dbo.Hotels WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = {hotelId} AND OwnerId = {ownerId} AND (IsDeleted = 0 OR IsDeleted IS NULL)")
            .AsTracking().SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int?> GetMaximumDisplayOrderAsync(int hotelId, CancellationToken cancellationToken = default) =>
        await _dbSet.Where(image => image.HotelId == hotelId).MaxAsync(image => (int?)image.DisplayOrder, cancellationToken);

    public async Task<int> GetAdditionalImageCountAsync(int hotelId, string? coverUrl, CancellationToken cancellationToken = default) =>
        await _dbSet.Where(image => image.HotelId == hotelId && image.IsDeleted != true && image.ImageUrl.Trim() != "" && image.ImageUrl != coverUrl)
            .Select(image => image.ImageUrl).Distinct().CountAsync(cancellationToken);

    public async Task AddGalleryImageAsync(HotelImage image, CancellationToken cancellationToken = default) =>
        await _dbSet.AddAsync(image, cancellationToken);

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
