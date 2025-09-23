using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using HotelBooking.api.Models;

namespace HotelBooking.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        IHotelService _hotelService;
        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [Authorize(Roles = "Owner")]
        [HttpGet("get-owner-dashboard")]
        public async Task<IActionResult> GetOwnerDashboardAsync()
        {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _hotelService.GetOwnerDashBoard(ownerId);
            return Ok(response);
        }

        [HttpGet("get-search-options")]
        public async Task<IActionResult> GetSearchOptionsAsync([FromQuery] string cityName,
        [FromQuery] DateTime? checkIn,
        [FromQuery] DateTime? checkOut,
        [FromQuery] int? adults,
        [FromQuery] int? children,
        [FromQuery] int? rooms)
        {
            var response = await _hotelService.GetSearchOptionsAsync(cityName, checkIn, checkOut, adults, children, rooms);
            return Ok(response);
        }

    }
}