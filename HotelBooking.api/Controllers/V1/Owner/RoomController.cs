using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;
using HotelBooking.application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBooking.api.Controllers.V1.Owner;

[Route("api/v1/rooms")]
[ApiController]
[Authorize(Roles = "Owner")]
[Tags("Owner - Rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("batch")]
    public async Task<IActionResult> BatchAddRooms([FromBody] BatchAddRoomsRequestDTO request, CancellationToken cancellationToken = default)
    {
        if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
        var result = await _roomService.BatchAddRoomsAsync(request, ownerId, cancellationToken);
        return ApiResponseHandlerHelper.HandleResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetRoomsByRoomType([FromQuery] int roomTypeId, CancellationToken cancellationToken = default)
    {
        if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
        var result = await _roomService.GetRoomsByRoomTypeAsync(roomTypeId, ownerId, cancellationToken);
        return ApiResponseHandlerHelper.HandleResponse(result);
    }

    private bool TryGetOwnerId(out int ownerId) =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out ownerId) && ownerId > 0;
}
