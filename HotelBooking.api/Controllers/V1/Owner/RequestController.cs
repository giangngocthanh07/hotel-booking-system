
using System.Security.Claims;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Owner
{
    [Route("api/v1/owner/hotel-registrations")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Tags("Owner - Request")]
    public class RequestController : ControllerBase
    {
        private readonly IHotelRegistrationService _hotelRegistrationService;
        private readonly IHotelRegistrationHistoryService _history;
        public RequestController(IHotelRegistrationService hotelRegistrationService, IHotelRegistrationHistoryService history)
        {
            _hotelRegistrationService = hotelRegistrationService;
            _history = history;
        }

        [HttpPost]
        public async Task<IActionResult> HotelRegistration([FromBody] HotelRegistrationDTO request)
        {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _hotelRegistrationService.CreateRequestAsync(ownerId, request);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetMyHotelRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int ownerId))
            {
                return Unauthorized();
            }

            var result = await _hotelRegistrationService.GetMyRequestsAsync(ownerId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpGet("me/paged")]
        public async Task<IActionResult> GetMyHistoryPage([FromQuery] PagingRequest paging, CancellationToken cancellationToken)
        {
            // FR-ONB-013: the caller cannot choose another Owner's history via query parameters.
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var ownerId) || ownerId <= 0)
                return Unauthorized();
            var result = await _history.GetPageAsync(ownerId, paging, cancellationToken);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
    }
}
