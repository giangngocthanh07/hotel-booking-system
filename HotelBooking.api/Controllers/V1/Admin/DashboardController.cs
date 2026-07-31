using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Interfaces;
using HotelBooking.application.Helpers;

namespace HotelBooking.api.Controllers.V1.Admin;

[Route("api/v1/dashboard")]
[ApiController]
[Authorize(Roles = "Admin")]
[Tags("Admin - Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var response = await _dashboardService.GetAdminDashboardStatsAsync();
        return ApiResponseHandlerHelper.HandleResponse(response);
    }
}
