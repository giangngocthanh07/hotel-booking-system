using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
            var adminId = GetAdminId();
            var result = await _adminHotelApprovalRequestService.ApproveRequestAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("{requestId:int}/reject")]
        public async Task<IActionResult> Reject(int requestId)
        {
            var adminId = GetAdminId();
            var result = await _adminHotelApprovalRequestService.RejectRequestAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        private int GetAdminId()
        {
            var claim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return claim;
        }

    }
}