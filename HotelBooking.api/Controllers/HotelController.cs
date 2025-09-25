using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using HotelBooking.api.Helpers;
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

        // ================= TÌM KIẾM KHÁCH SẠN THEO FILTER SearchForm.razor ================

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

        // =============== ĐỌC, THÊM, SỬA, XÓA TIỆN ÍCH CHO KHÁCH SẠN ================
        // [Authorize(Roles = "Admin")]
        [HttpGet("get-all-amenities")]
        public async Task<IActionResult> GetAllAmenitiesAsync()
        {
            var response = await _hotelService.GetAllAmenitiesAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }


        // [Authorize(Roles = "Admin")]
        [HttpPost("create-amenity")]
        public async Task<IActionResult> CreateAmenityAsync([FromBody] AmenityCreateOrUpdateDTO newAmenity)
        {
            var response = await _hotelService.CreateAmenityAsync(newAmenity);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPut("update-amenity/{id}")]
        public async Task<IActionResult> UpdateAmenityAsync(int id, [FromBody] AmenityCreateOrUpdateDTO amenity)
        {

            var response = await _hotelService.UpdateAmenityAsync(id, amenity);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpDelete("delete-amenity/{id}")]
        public async Task<IActionResult> DeleteAmenityAsync(int id)
        {
            var response = await _hotelService.DeleteAmenityAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

    }
}