
using System.Security.Claims;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Owner
{
    [Route("api/v1/owner")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Tags("Owner - Request")]
    public class RequestController : ControllerBase
    {
        private readonly IHotelRegistrationService _hotelRegistrationService;
        public RequestController(IHotelRegistrationService hotelRegistrationService)
        {
            _hotelRegistrationService = hotelRegistrationService;
        }

        [HttpPost("hotel-registration")]
        public async Task<IActionResult> HotelRegistration([FromBody] HotelRegistrationDTO request)
        {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _hotelRegistrationService.CreateRequestAsync(ownerId, request);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpGet("my-hotel-requests")]
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
    }
}