using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.RequestManagement;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin Upgrade Requests Controller - Admin quản lý các đơn yêu cầu nâng cấp từ khách hàng
    /// </summary>
    [Route("api/v1/admin/upgrade-requests")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Tags("Admin - Upgrade Requests")]
    public class UpgradeRequestController : ControllerBase
    {
        private readonly IUpgradeRequestService _upgradeRequestService;

        public UpgradeRequestController(IUpgradeRequestService upgradeRequestService)
        {
            _upgradeRequestService = upgradeRequestService;
        }

        /// <summary>
        /// Lấy danh sách các Status từ DB (để Swagger biết mà nhập)
        /// </summary>
        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var response = await _upgradeRequestService.GetAllStatusesAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Lấy danh sách Request có phân trang (Reuse PagingRequest)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPagedRequestsAsync([FromQuery] PagingRequest pagingRequest, [FromQuery] string? status)
        {
            var response = await _upgradeRequestService.GetPagedRequestsAsync(pagingRequest, status);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [HttpGet("get-all-requests")]
        // [Obsolete("Use GetPagedRequestsAsync instead for better performance")]
        // public async Task<IActionResult> GetRequestsAsync([FromQuery] string? status)
        // {
        //     var response = await _upgradeRequestService.GetAllRequestAsync(status);
        //     return ApiResponseHandlerHelper.HandleResponse(response);
        // }

        /// <summary>
        /// Lấy chi tiết request theo ID
        /// </summary>
        [HttpGet("{requestId:int}")]
        public async Task<IActionResult> GetByRequestIdAsync(int requestId)
        {
            var response = await _upgradeRequestService.GetByRequestIdAsync(requestId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Duyệt request
        /// </summary>
        [HttpPost("{requestId:int}/approve")]
        public async Task<IActionResult> Approve(int requestId)
        {
            var adminId = GetAdminId();
            if (adminId == 0) return Unauthorized();

            var response = await _upgradeRequestService.ApproveRequestAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Từ chối request
        /// </summary>
        [HttpPost("{requestId:int}/reject")]
        public async Task<IActionResult> Reject(int requestId)
        {
            var adminId = GetAdminId();
            if (adminId == 0) return Unauthorized();

            var response = await _upgradeRequestService.RejectRequestAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        private int GetAdminId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
