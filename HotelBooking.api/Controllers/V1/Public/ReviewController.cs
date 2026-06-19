using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Interfaces;
using HotelBooking.application.DTOs.Hotel;
using System.Security.Claims;
using HotelBooking.application.Helpers;

namespace HotelBooking.api.Controllers.V1.Public;

[Route("api/v1/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize]
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitReview([FromBody] ReviewRequestDTO request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        int userId = int.Parse(userIdClaim.Value);
        var response = await _reviewService.SubmitReviewAsync(request, userId);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }

    [HttpGet("hotel/{hotelId}")]
    public async Task<IActionResult> GetHotelReviews(int hotelId)
    {
        var response = await _reviewService.GetHotelReviewsAsync(hotelId);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("reply")]
    public async Task<IActionResult> ReplyToReview([FromBody] ReviewReplyRequestDTO request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _reviewService.ReplyToReviewAsync(request, userId);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("hide")]
    public async Task<IActionResult> HideReview([FromBody] ReviewModerationRequestDTO request)
    {
        var response = await _reviewService.HideReviewAsync(request);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }
}
