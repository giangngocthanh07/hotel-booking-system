
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotelBooking.application.Services.Domains.RequestManagement.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using V1.Models;

namespace HotelBooking.API.Controllers.V1.Owner
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Tags("Owner - Hotel Services")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelRegistrationService _hotelRegistrationService;

        public HotelController(IHotelRegistrationService hotelRegistrationService)
        {
            _hotelRegistrationService = hotelRegistrationService;
        }

        [HttpPost("hotel-registration")]
        public async Task<IActionResult> HotelRegistration([FromBody] HotelRegistrationDTO request)
        {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _hotelRegistrationService.HotelRegistrationAsync(request, ownerId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

    }
}