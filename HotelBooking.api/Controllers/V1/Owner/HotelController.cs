using System.Security.Claims;
using HotelBooking.application.DTOs.Hotel.Owner;
using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Owner;

[Route("api/v1/owner/hotels")]
[ApiController]
[Authorize(Roles = "Owner")]
[Tags("Owner - Hotels")]
public class HotelController(IOwnerHotelService hotels) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPage([FromQuery] PagingRequest paging, CancellationToken cancellationToken = default)
    {
        // FR-CAT-001/003: the caller cannot choose an Owner ID through route or query parameters.
        if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
        return ApiResponseHandlerHelper.HandleResponse(await hotels.GetPageAsync(ownerId, paging, cancellationToken));
    }

    [HttpGet("{hotelId:int}")]
    public async Task<IActionResult> GetById(int hotelId, CancellationToken cancellationToken = default)
    {
        if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
        return ApiResponseHandlerHelper.HandleResponse(await hotels.GetByIdAsync(ownerId, hotelId, cancellationToken));
    }

    [HttpPut("{hotelId:int}/information")]
    public async Task<IActionResult> UpdateInformation(int hotelId, [FromBody] OwnerHotelUpdateDTO input,
        [FromServices] IOwnerHotelUpdateService updater, CancellationToken cancellationToken = default)
    {
        // FR-CAT-002/003: the Owner identity comes only from trusted authentication claims.
        if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
        return ApiResponseHandlerHelper.HandleResponse(await updater.UpdateAsync(ownerId, hotelId, input, cancellationToken));
    }

    private bool TryGetOwnerId(out int ownerId) =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out ownerId) && ownerId > 0;
}
