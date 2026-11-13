using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Validators.HotelManagement.Owner;

namespace HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;

public interface IOwnerHotelImageDeleteService
{
    Task<ApiResponse<bool>> DeleteAsync(int ownerId, int hotelId, int imageId, CancellationToken cancellationToken = default);
}

public class OwnerHotelImageDeleteService(IHotelImageRepository images, IUnitOfWork unit,
    ILogger<OwnerHotelImageDeleteService> logger) : IOwnerHotelImageDeleteService
{
    private readonly OwnerHotelIdValidator _validator = new();

    public async Task<ApiResponse<bool>> DeleteAsync(int ownerId, int hotelId, int imageId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return ResponseFactory.Failure<bool>(StatusCodeResponse.Unauthorized, "Invalid Owner identity.");
        if (!(await _validator.ValidateAsync(hotelId, cancellationToken)).IsValid || !(await _validator.ValidateAsync(imageId, cancellationToken)).IsValid)
            return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, "Invalid hotel or image ID.");
        try { return await DeleteOwnedAsync(ownerId, hotelId, imageId, cancellationToken); }
        catch (OperationCanceledException) { throw; }
        catch (Exception error)
        {
            logger.LogError(error, "Gallery deletion failed for Owner {OwnerId}, Hotel {HotelId}, Image {ImageId}.", ownerId, hotelId, imageId);
            return ResponseFactory.ServerError<bool>();
        }
    }

    private async Task<ApiResponse<bool>> DeleteOwnedAsync(int ownerId, int hotelId, int imageId, CancellationToken token)
    {
        await unit.BeginTransactionAsync(token);
        try
        {
            // FR-CAT-003 / FR-MED-005: share the upload lock to serialize removal, replacement and gallery counts.
            var hotel = await images.GetOwnedForMediaWriteAsync(ownerId, hotelId, token);
            if (hotel == null) return await AbortAsync(StatusCodeResponse.NotFound, "Hotel image not found.");
            var image = await images.GetGalleryImageForWriteAsync(hotelId, imageId, token);
            if (image == null) return await AbortAsync(StatusCodeResponse.NotFound, "Hotel image not found.");
            if (!await CanRemoveAsync(hotel.Id, hotel.Status, hotel.CoverImageUrl, imageId, token))
                return await AbortAsync(StatusCodeResponse.Conflict, "An Active hotel needs at least 4 different gallery images in addition to its cover. Add a replacement image before removing this one.");
            image.IsDeleted = true;
            return await SaveAsync(token);
        }
        catch { await unit.RollBackTransactionAsync(CancellationToken.None); throw; }
    }

    private async Task<bool> CanRemoveAsync(int hotelId, string? status, string? cover, int imageId, CancellationToken token)
    {
        if (status != HotelStatus.Active) return true;
        // FR-CAT-006: duplicate URLs and the cover do not count toward readiness; do not worsen legacy shortfalls.
        var before = await images.GetAdditionalImageCountAsync(hotelId, cover, token);
        var after = await images.GetAdditionalImageCountExcludingAsync(hotelId, cover, imageId, token);
        return after >= Math.Min(4, before);
    }

    private async Task<ApiResponse<bool>> SaveAsync(CancellationToken token)
    {
        var saved = await unit.TrySaveChangesAsync(token);
        if (saved.HasConcurrencyConflict || saved.AffectedRows < 1)
            return await AbortAsync(saved.HasConcurrencyConflict ? StatusCodeResponse.Conflict : StatusCodeResponse.Error, "Unable to remove image. Refresh and try again.");
        await unit.CommitTransactionAsync(token);
        return ResponseFactory.Success(true, "Gallery image removed.");
    }

    private async Task<ApiResponse<bool>> AbortAsync(string status, string message)
    {
        await unit.RollBackTransactionAsync(CancellationToken.None);
        return ResponseFactory.Failure<bool>(status, message);
    }
}
