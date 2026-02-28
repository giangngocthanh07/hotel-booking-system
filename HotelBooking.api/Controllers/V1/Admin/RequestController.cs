using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.application.Helpers;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin Requests Controller - Admin quản lý các đơn yêu cầu từ khách hàng
    /// </summary>
    [Route("api/v1/admin/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IUpgradeRequestService _upgradeRequestService;

        public RequestController(IUpgradeRequestService upgradeRequestService)
        {
            _upgradeRequestService = upgradeRequestService;
        }

        /// <summary>
        /// Lấy danh sách Request có phân trang (Reuse PagingRequest)
        /// </summary>
        [HttpGet("get-requests-paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPagedRequestsAsync([FromQuery] PagingRequest pagingRequest, [FromQuery] string? status)
        {
            var response = await _upgradeRequestService.GetPagedRequestsAsync(pagingRequest, status);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpGet("get-all-requests")]
        [Authorize(Roles = "Admin")]
        [Obsolete("Use GetPagedRequestsAsync instead for better performance")]
        public async Task<IActionResult> GetRequestsAsync([FromQuery] string? status)
        {
            var response = await _upgradeRequestService.GetAllRequestAsync(status);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpGet("request-detail/{requestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByRequestIdAsync(int requestId)
        {
            var response = await _upgradeRequestService.GetByRequestIdAsync(requestId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPost("approve/{requestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUpgradeAsync(int requestId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid");
            if (claim == null) return BadRequest("AdminId claim is missing.");

            var adminId = int.Parse(claim.Value);
            var response = await _upgradeRequestService.ApproveRequestAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPost("reject/{requestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectUpgradeAsync(int requestId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid");
            if (claim == null) return BadRequest("AdminId claim is missing.");

            var adminId = int.Parse(claim.Value);
            var response = await _upgradeRequestService.RejectRequestAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
    }
}
