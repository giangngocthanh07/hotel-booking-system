using System.Security.Claims;
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
}
