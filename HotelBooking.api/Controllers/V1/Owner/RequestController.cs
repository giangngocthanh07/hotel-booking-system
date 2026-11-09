
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
        public async Task<IActionResult> HotelRegistration([FromBody] HotelRegistrationDTO request, CancellationToken cancellationToken = default)
        {
            // FR-ONB-009: reject invalid identities before creating an Owner-scoped request.
            if (!TryGetOwnerId(out var ownerId)) return Unauthorized();

            var result = await _hotelRegistrationService.CreateRequestAsync(ownerId, request, cancellationToken);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetMyHotelRequests()
        {
            // FR-ONB-013: history must be scoped to a valid authenticated Owner.
            if (!TryGetOwnerId(out var ownerId)) return Unauthorized();

            var result = await _hotelRegistrationService.GetMyRequestsAsync(ownerId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpGet("me/paged")]
        public async Task<IActionResult> GetMyHistoryPage([FromQuery] PagingRequest paging, CancellationToken cancellationToken)
        {
            // FR-ONB-013: the caller cannot choose another Owner's history via query parameters.
            if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
            var result = await _history.GetPageAsync(ownerId, paging, cancellationToken);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("{requestId:int}/cancel")]
        public async Task<IActionResult> Cancel(int requestId)
        {
            // FR-ONB-018: use only the authenticated Owner, never a caller-supplied user ID.
            if (!TryGetOwnerId(out var ownerId)) return Unauthorized();
            var result = await _hotelRegistrationService.CancelRequestAsync(ownerId, requestId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        private bool TryGetOwnerId(out int ownerId) =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out ownerId) && ownerId > 0;
    }
}
