using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Interfaces;
using HotelBooking.application.Helpers;
using System.Security.Claims;

namespace HotelBooking.api.Controllers.V1.Owner;

[Route("api/v1/owner-dashboard")]
[ApiController]
[Authorize(Roles = "Owner")]
[Tags("Owner - Dashboard")]
public class OwnerDashboardController : ControllerBase
{
    private readonly IOwnerDashboardService _dashboardService;

    public OwnerDashboardController(IOwnerDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        int ownerId = int.Parse(userIdClaim.Value);
        var response = await _dashboardService.GetOwnerDashboardStatsAsync(ownerId);
        return ApiResponseHandlerHelper.HandleResponse(response);
    }
}
