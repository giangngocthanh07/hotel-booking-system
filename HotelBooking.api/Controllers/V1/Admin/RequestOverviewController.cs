using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.application.Helpers;
using HotelBooking.application.DTOs.Request.Overview;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin - Tổng quan tất cả loại requests (Dashboard)
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
        /// Thống kê tổng quan tất cả loại requests
        /// </summary>
        /// <remarks>
        /// Trả về stats của:
        /// - Upgrade Owner requests
        /// - Hotel Approval requests (sau này)
        /// - Tổng pending, tổng hôm nay
        /// </remarks>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<RequestStatsDTO>), 200)]
        public async Task<IActionResult> GetStats()
        {
            var response = await _overviewService.GetStatsAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Lấy requests gần đây (tất cả loại) - Widget Dashboard
        /// </summary>
        /// <param name="count">Số lượng request muốn lấy (default: 10)</param>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(ApiResponse<List<RecentRequestDTO>>), 200)]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
        {
            var response = await _overviewService.GetRecentRequestsAsync(count);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
    }
}