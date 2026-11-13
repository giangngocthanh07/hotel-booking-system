using System.Security.Claims;
using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Services.Helpers.Media.HotelImages;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Owner.Images;

[ApiController]
[Authorize(Roles = "Owner")]
[Route("api/v1/owner/hotels/{hotelId:int}/images")]
[Tags("Owner - Hotel Images")]
public class HotelImagesController(IOwnerHotelImageReadService images) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int hotelId, CancellationToken cancellationToken = default)
    {
        // FR-CAT-003: ownership is derived from authenticated claims, not route/query Owner IDs.
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var ownerId) || ownerId <= 0)
            return Unauthorized();
        return ApiResponseHandlerHelper.HandleResponse(await images.GetAsync(ownerId, hotelId, cancellationToken));
    }

    [HttpPost("cover")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(HotelImageFileRules.MaximumSize + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = HotelImageFileRules.MaximumSize + 1024 * 1024)]
    public Task<IActionResult> UploadCover(int hotelId, IFormFile? file, [FromServices] IOwnerHotelImageUploadService uploader,
        CancellationToken cancellationToken = default) => Upload(hotelId, file, uploader, true, cancellationToken);

    [HttpDelete("gallery/{imageId:int}")]
    public async Task<IActionResult> DeleteGallery(int hotelId, int imageId, [FromServices] IOwnerHotelImageDeleteService remover,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var ownerId) || ownerId <= 0) return Unauthorized();
        return ApiResponseHandlerHelper.HandleResponse(await remover.DeleteAsync(ownerId, hotelId, imageId, cancellationToken));
    }

    [HttpPost("gallery")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(HotelImageFileRules.MaximumSize + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = HotelImageFileRules.MaximumSize + 1024 * 1024)]
    public Task<IActionResult> UploadGallery(int hotelId, IFormFile? file, [FromServices] IOwnerHotelImageUploadService uploader,
        CancellationToken cancellationToken = default) => Upload(hotelId, file, uploader, false, cancellationToken);

    private async Task<IActionResult> Upload(int hotelId, IFormFile? file, IOwnerHotelImageUploadService uploader,
        bool cover, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var ownerId) || ownerId <= 0) return Unauthorized();
        if (file == null) return ApiResponseHandlerHelper.HandleResponse(
            ResponseFactory.Failure<OwnerHotelImageUploadDTO>(StatusCodeResponse.BadRequest, "An image file is required."));
        await using var stream = file.OpenReadStream();
        var input = new OwnerHotelImageFileDTO { FileName = file.FileName, ContentType = file.ContentType, Size = file.Length, Content = stream };
        return ApiResponseHandlerHelper.HandleResponse(await uploader.UploadAsync(ownerId, hotelId, input, cover, cancellationToken));
    }
}
