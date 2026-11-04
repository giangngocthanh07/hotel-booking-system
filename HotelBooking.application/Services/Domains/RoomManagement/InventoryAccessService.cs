using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.RoomManagement;

// Centralizes ownership checks so direct application-service calls remain protected.
public interface IInventoryAccessService
{
    Task<bool> CanAccessHotelAsync(int hotelId, int ownerId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessRoomTypeAsync(int roomTypeId, int ownerId, int? hotelId = null, CancellationToken cancellationToken = default);
}

public class InventoryAccessService(IHotelRepository hotelRepository, IRoomTypeRepository roomTypeRepository) : IInventoryAccessService
{
    // Use the same public message for missing, deleted, and foreign-owned resources.
    public const string AccessDeniedMessage = "You do not have access to this inventory.";

    public async Task<bool> CanAccessHotelAsync(int hotelId, int ownerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (hotelId <= 0 || ownerId <= 0) return false;
        // Owners may configure inventory before publication; ownership and soft deletion govern access.
        return await hotelRepository.AnyAsync(hotel => hotel.Id == hotelId
            && hotel.OwnerId == ownerId && hotel.IsDeleted != true);
    }

    public async Task<bool> CanAccessRoomTypeAsync(int roomTypeId, int ownerId, int? hotelId = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (roomTypeId <= 0 || ownerId <= 0 || hotelId <= 0) return false;
        // Derive ownership from the persisted parent Hotel, never from a client-supplied owner ID.
        // Optional HotelId additionally prevents a batch request from mixing unrelated resources.
        return await roomTypeRepository.AnyAsync(roomType => roomType.Id == roomTypeId
            && roomType.IsDeleted != true && roomType.Hotel.IsDeleted != true
            && roomType.Hotel.OwnerId == ownerId && (!hotelId.HasValue || roomType.HotelId == hotelId.Value));
    }
}
