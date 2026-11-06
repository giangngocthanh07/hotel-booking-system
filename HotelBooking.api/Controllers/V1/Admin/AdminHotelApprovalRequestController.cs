using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using V1.Models;

namespace V1.Controllers
{
    [Route("api/v1/admin/hotel-approvals")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Tags("Admin - Hotel Approval Requests")]
    public class AdminHotelApprovalRequestController : ControllerBase
    {
        private readonly IAdminHotelApprovalRequestService _adminHotelApprovalRequestService;

        public AdminHotelApprovalRequestController(IAdminHotelApprovalRequestService adminHotelApprovalRequestService)
        {
            _adminHotelApprovalRequestService = adminHotelApprovalRequestService;
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var result = await _adminHotelApprovalRequestService.GetAllStatusesAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedRequestsAsync([FromQuery] PagingRequest pagingRequest, [FromQuery] string? status)
        {
            var result = await _adminHotelApprovalRequestService.GetPagedRequestsAsync(pagingRequest, status);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
        [HttpGet("{requestId:int}")]
        public async Task<IActionResult> GetByRequestIdAsync(int requestId)
        {
            var result = await _adminHotelApprovalRequestService.GetByRequestIdAsync(requestId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("{requestId:int}/approve")]
        public async Task<IActionResult> Approve(int requestId)
        {
            // FR-ONB-015: a decision requires a valid authenticated Admin identity.
            if (!TryGetAdminId(out var adminId)) return Unauthorized();
            var result = await _adminHotelApprovalRequestService.ApproveRequestAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("{requestId:int}/reject")]
        // Bind the explanation from JSON; the acting Admin still comes from authentication claims.
        // Older clients posting no body must be updated to send { "reason": "..." }.
        public async Task<IActionResult> Reject(int requestId, [FromBody] RejectHotelRegistrationDTO request, CancellationToken cancellationToken = default)
        {
            // FR-ONB-017: never delegate rejection with a missing or malformed Admin ID.
            if (!TryGetAdminId(out var adminId)) return Unauthorized();
            var result = await _adminHotelApprovalRequestService.RejectRequestAsync(requestId, adminId, request, cancellationToken);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        private bool TryGetAdminId(out int adminId) =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out adminId) && adminId > 0;

    }
}
