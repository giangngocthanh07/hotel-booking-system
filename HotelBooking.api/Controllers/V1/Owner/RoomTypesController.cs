using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RoomManagement;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
// Nhớ using các namespace chứa Service, DTO và ResponseFactory của bạn

namespace HotelBooking.API.Controllers
{
    [Route("api/room-types")]
    [ApiController]
    public class RoomTypesController : ControllerBase
    {
        private readonly IRoomNameSuggestionService _suggestionService;

        // Dependency Injection
        public RoomTypesController(IRoomNameSuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        /// <summary>
        /// Generate room name suggestions based on selected attributes
        /// </summary>
        [HttpPost("suggest-names")]
        public async Task<IActionResult> SuggestRoomNames([FromBody] RoomNameSuggestionRequest request)
        {
            // Call the service layer to get data
            var result = await _suggestionService.SuggestRoomNamesAsync(request);

            // Depending on how you design the signature of SuggestRoomNamesAsync:
            // 1. If the method returns List<string>, you can wrap it using ResponseFactory:
            // return Ok(ResponseFactory.Success(result));

            // 2. If your method (as in the test file) already returns ApiResponse<List<string>>:
            return Ok(result);
        }
    }
}