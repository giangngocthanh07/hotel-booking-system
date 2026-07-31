using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Interfaces;
using HotelBooking.application.DTOs.Booking;
using System.Security.Claims;
using HotelBooking.application.Helpers;

namespace HotelBooking.api.Controllers.V1.Public;

[Route("api/v1/bookings")]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateBookingAsync([FromBody] BookingRequestDTO request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        int userId = int.Parse(userIdClaim.Value);
        var response = await _bookingService.CreateBookingAsync(request, userId);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBookingHistory([FromQuery] string? status)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _bookingService.GetGuestBookingsAsync(userId, status);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("owner")]
    public async Task<IActionResult> GetOwnerBookingManagement([FromQuery] string? status, [FromQuery] string? searchTerm)
    {
        var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _bookingService.GetOwnerBookingsAsync(ownerId, status, searchTerm);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }
}
