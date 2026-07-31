using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;
using HotelBooking.application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> BatchAddRooms([FromBody] BatchAddRoomsRequestDTO request)
    {
        var result = await _roomService.BatchAddRoomsAsync(request);
        return ApiResponseHandlerHelper.HandleResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetRoomsByRoomType([FromQuery] int roomTypeId)
    {
        var result = await _roomService.GetRoomsByRoomTypeAsync(roomTypeId);
        return ApiResponseHandlerHelper.HandleResponse(result);
    }
}
