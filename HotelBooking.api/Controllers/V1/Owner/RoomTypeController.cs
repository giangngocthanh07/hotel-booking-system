using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.RoomManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBooking.API.Controllers.V1.Owner
{
    [Route("api/v1/room-types")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Tags("Owner - Room Types")]
    public class RoomTypeController : ControllerBase
    {
        private readonly IRoomNameSuggestionService _suggestionService;
        private readonly IRoomTypeService _roomTypeService;

        // Dependency Injection
        public RoomTypeController(IRoomNameSuggestionService suggestionService, IRoomTypeService roomTypeService)
        {
            _suggestionService = suggestionService;
            _roomTypeService = roomTypeService;
        }

        /// <summary>
        /// Generate room name suggestions based on selected attributes
        /// </summary>
        [HttpPost("name-suggestions")]
        public async Task<IActionResult> SuggestRoomNames([FromBody] RoomNameSuggestionRequest request)
        {
            // Call the service layer to get data
            var result = await _suggestionService.SuggestRoomNamesAsync(request);

            // Depending on how you design the signature of SuggestRoomNamesAsync:
            // 1. If the method returns List<string>, you can wrap it using ResponseFactory:
            // return Ok(ResponseFactory.Success(result));

            // 2. If your method (as in the test file) already returns ApiResponse<List<string>>:
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoomType([FromBody] RoomTypeCreateDTO request, CancellationToken cancellationToken = default)
        {
            if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
            var result = await _roomTypeService.CreateRoomTypeAsync(request, ownerId, cancellationToken);

            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        private bool TryGetOwnerId(out int ownerId) =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out ownerId) && ownerId > 0;
    }
}
