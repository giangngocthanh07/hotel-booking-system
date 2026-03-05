using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.application.Helpers;
using HotelBooking.application.DTOs.Request.Overview;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin - Overview of all request types (Dashboard)
    /// </summary>
    [Route("api/v1/admin/requests")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Tags("Admin - Request Overview")]
    public class RequestOverviewController : ControllerBase
    {
        private readonly IRequestOverviewService _overviewService;

        public RequestOverviewController(IRequestOverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        /// <summary>
        /// Overview statistics of all request types
        /// </summary>
        /// <remarks>
        /// Returns stats of:
        /// - Upgrade Owner requests
        /// - Hotel Approval requests (later)
        /// - Total pending, total today
        /// </remarks>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<RequestStatsDTO>), 200)]
        public async Task<IActionResult> GetStats()
        {
            var response = await _overviewService.GetStatsAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Get recent requests (all types) - Dashboard Widget
        /// </summary>
        /// <param name="count">Number of requests to retrieve (default: 10)</param>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(ApiResponse<List<RecentRequestDTO>>), 200)]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
        {
            var response = await _overviewService.GetRecentRequestsAsync(count);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
    }
}