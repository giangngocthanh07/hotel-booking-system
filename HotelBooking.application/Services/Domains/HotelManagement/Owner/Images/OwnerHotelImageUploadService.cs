using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Services.Helpers.Media;
using HotelBooking.application.Services.Helpers.Media.HotelImages;
using HotelBooking.application.Validators.HotelManagement.Owner;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;

public interface IOwnerHotelImageUploadService
{
    Task<ApiResponse<OwnerHotelImageUploadDTO>> UploadAsync(int ownerId, int hotelId, OwnerHotelImageFileDTO? file,
        bool cover, CancellationToken cancellationToken = default);
}

public class OwnerHotelImageUploadService(IHotelImageRepository images, IPhotoService photos, IUnitOfWork unit,
    IValidator<OwnerHotelImageFileDTO> validator, ILogger<OwnerHotelImageUploadService> logger) : IOwnerHotelImageUploadService
{
    private readonly OwnerHotelIdValidator _idValidator = new();

    public async Task<ApiResponse<OwnerHotelImageUploadDTO>> UploadAsync(int ownerId, int hotelId, OwnerHotelImageFileDTO? file,
        bool cover, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return Failure(StatusCodeResponse.Unauthorized, "Invalid Owner identity.");
        if (file == null) return Invalid("An image file is required.");
        var id = await _idValidator.ValidateAsync(hotelId, cancellationToken);
        if (!id.IsValid) return Invalid(id.Errors[0].ErrorMessage);
        var validation = await validator.ValidateAsync(file, cancellationToken);
        if (!validation.IsValid) return Invalid(validation.Errors[0].ErrorMessage);
        try { return await UploadOwnedAsync(ownerId, hotelId, file, cover, cancellationToken); }
        catch (OperationCanceledException) { throw; }
        catch (Exception error)
        {
            logger.LogError(error, "Hotel image upload failed for Owner {OwnerId}, Hotel {HotelId}.", ownerId, hotelId);
            return ResponseFactory.ServerError<OwnerHotelImageUploadDTO>();
        }
    }

    private async Task<ApiResponse<OwnerHotelImageUploadDTO>> UploadOwnedAsync(int ownerId, int hotelId,
        OwnerHotelImageFileDTO file, bool cover, CancellationToken cancellationToken)
    {
        // FR-CAT-003 / FR-MED-003: do not read a foreign Hotel's file or reach the provider before ownership checks.
        if (!await images.OwnsHotelAsync(ownerId, hotelId, cancellationToken)) return Missing();
        await using var content = await BoundedMediaContent.ReadAsync(file.Content, HotelImageFileRules.MaximumSize, cancellationToken);
        if (content == null) return Invalid("This photo is too large. Choose a JPG, JPEG or PNG photo no larger than 10 MB.");
        if (content.Length == 0 || content.Length != file.Size) return Invalid("This image is empty or could not be read completely. Choose the original JPG, JPEG or PNG image, up to 10 MB, and try again.");
        if (!HotelImageFileRules.MatchesSignature(file.FileName, content.GetBuffer().AsSpan(0, (int)content.Length)))
            return Invalid("This file is not a valid JPG, JPEG or PNG image. Open and export it as JPG or PNG, then upload it again (maximum 10 MB). Simply renaming the file is not enough.");
        var uploaded = await photos.UploadHotelImageAsync(new UploadFileDTO
        {
            FileName = file.FileName, ContentType = file.ContentType, Size = content.Length, Content = content,
        }, ownerId, hotelId, cover, cancellationToken);
        if (uploaded.StatusCode != StatusCodeResponse.Success) return Failure(uploaded.StatusCode, uploaded.Message);
        var url = uploaded.Content?.StoredFileName;
        if (uploaded.Content?.Uploaded != true || !ValidUrl(url)) return ResponseFactory.ServerError<OwnerHotelImageUploadDTO>();
        return await PersistAsync(ownerId, hotelId, url!, cover, cancellationToken);
    }

    private async Task<ApiResponse<OwnerHotelImageUploadDTO>> PersistAsync(int ownerId, int hotelId, string url,
        bool cover, CancellationToken cancellationToken)
    {
        // FR-MED-005: provider work finishes before acquiring SQL locks; recheck ownership under the write lock.
        await unit.BeginTransactionAsync(cancellationToken);
        try
        {
            var hotel = await images.GetOwnedForMediaWriteAsync(ownerId, hotelId, cancellationToken);
            if (hotel == null) return await AbortAsync(StatusCodeResponse.NotFound, "Hotel not found.");
            var (image, error) = await PrepareAsync(hotel, url, cover, cancellationToken);
            if (error != null) return await AbortAsync(StatusCodeResponse.Conflict, error);
            var saved = await unit.TrySaveChangesAsync(cancellationToken);
            if (saved.HasConcurrencyConflict || saved.AffectedRows < 1)
                return await AbortAsync(saved.HasConcurrencyConflict ? StatusCodeResponse.Conflict : StatusCodeResponse.Error, "Unable to save hotel image. Refresh and try again.");
            await unit.CommitTransactionAsync(cancellationToken);
            return ResponseFactory.Success(new OwnerHotelImageUploadDTO
                { HotelId = hotelId, ImageUrl = url, ImageId = image?.Id, DisplayOrder = image?.DisplayOrder }, "Hotel image uploaded.");
        }
        catch { await unit.RollBackTransactionAsync(CancellationToken.None); throw; }
    }

    private async Task<(HotelImage? Image, string? Error)> PrepareAsync(Hotel hotel, string url, bool cover, CancellationToken cancellationToken)
    {
        if (cover)
        {
            if (!await CanReplaceCoverAsync(hotel, url, cancellationToken))
                return (null, "This cover would leave too few additional gallery images for an Active Hotel.");
            hotel.CoverImageUrl = url;
            return (null, null);
        }
        var maximum = await images.GetMaximumDisplayOrderAsync(hotel.Id, cancellationToken);
        if (maximum == int.MaxValue) return (null, "Gallery ordering limit reached.");
        var image = new HotelImage { HotelId = hotel.Id, ImageUrl = url, DisplayOrder = (maximum ?? -1) + 1, IsDeleted = false };
        await images.AddGalleryImageAsync(image, cancellationToken);
        return (image, null);
    }

    private async Task<bool> CanReplaceCoverAsync(Hotel hotel, string url, CancellationToken cancellationToken)
    {
        if (hotel.Status != HotelStatus.Active) return true;
        // FR-CAT-006: retain four additional images; do not worsen already insufficient legacy galleries.
        var before = await images.GetAdditionalImageCountAsync(hotel.Id, hotel.CoverImageUrl, cancellationToken);
        var after = await images.GetAdditionalImageCountAsync(hotel.Id, url, cancellationToken);
        return after >= Math.Min(4, before);
    }

    private async Task<ApiResponse<OwnerHotelImageUploadDTO>> AbortAsync(string status, string? message)
    {
        // Cancellation must not prevent rollback and leave an image-write lock held.
        await unit.RollBackTransactionAsync(CancellationToken.None);
        return Failure(status, message);
    }

    private static bool ValidUrl(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 500
        && Uri.TryCreate(value, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttps;
    private static ApiResponse<OwnerHotelImageUploadDTO> Missing() => Failure(StatusCodeResponse.NotFound, "Hotel not found.");
    private static ApiResponse<OwnerHotelImageUploadDTO> Invalid(string message) => Failure(StatusCodeResponse.BadRequest, message);
    private static ApiResponse<OwnerHotelImageUploadDTO> Failure(string? status, string? message) => ResponseFactory.Failure<OwnerHotelImageUploadDTO>(status, message);
}
