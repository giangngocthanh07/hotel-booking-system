using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Interfaces;
using HotelBooking.application.DTOs.Booking;
using System.Security.Claims;
using HotelBooking.application.Helpers;

namespace HotelBooking.api.Controllers.V1.Public;

[Route("api/v1/payments")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Create a payment URL for a specific booking.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentRequestDTO request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await _paymentService.CreatePaymentUrlAsync(request, userId);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }

    /// <summary>
    /// Handle callback from payment gateway (Mocked IPN/Webhook).
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> PaymentCallback([FromBody] PaymentCallbackDTO callback)
    {
        var response = await _paymentService.ProcessPaymentCallbackAsync(callback);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }
}
