using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Validators.HotelManagement.Owner;

namespace HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;

public interface IOwnerHotelImageReadService
{
    Task<ApiResponse<OwnerHotelMediaDTO>> GetAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default);
}

public class OwnerHotelImageReadService(IHotelImageRepository images) : IOwnerHotelImageReadService
{
    private readonly OwnerHotelIdValidator _validator = new();

    public async Task<ApiResponse<OwnerHotelMediaDTO>> GetAsync(int ownerId, int hotelId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerId <= 0) return ResponseFactory.Failure<OwnerHotelMediaDTO>(StatusCodeResponse.Unauthorized, "Invalid Owner identity.");
        var validation = await _validator.ValidateAsync(hotelId, cancellationToken);
        if (!validation.IsValid) return ResponseFactory.Failure<OwnerHotelMediaDTO>(StatusCodeResponse.BadRequest, validation.Errors[0].ErrorMessage);
        // FR-CAT-003 / FR-MED-005/006: SQL scopes the owned Hotel and excludes deleted gallery rows before mapping.
        var hotel = await images.GetOwnedGalleryAsync(ownerId, hotelId, cancellationToken);
        if (hotel == null) return ResponseFactory.Failure<OwnerHotelMediaDTO>(StatusCodeResponse.NotFound, "Hotel not found.");
        return ResponseFactory.Success(new OwnerHotelMediaDTO
        {
            HotelId = hotel.Id, CoverImageUrl = hotel.CoverImageUrl,
            Gallery = hotel.HotelImages.Select(image => new OwnerHotelImageDTO
                { Id = image.Id, ImageUrl = image.ImageUrl, DisplayOrder = image.DisplayOrder }).ToList(),
        }, "Hotel images retrieved.");
    }
}
