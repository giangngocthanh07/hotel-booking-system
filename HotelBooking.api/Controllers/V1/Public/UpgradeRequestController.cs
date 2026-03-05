using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Services.Domains.RequestManagement;
using HotelBooking.application.Helpers;

namespace HotelBooking.api.Controllers.V1.Public
{
    /// <summary>
    /// Public Upgrade Request Controller - endpoints for customers
    /// </summary>
    [Route("api/v1/upgrade-requests")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    [Tags("Public - Upgrade Requests")]
    public class UpgradeRequestController : ControllerBase
    {
        private readonly IUpgradeRequestService _upgradeRequestService;

        public UpgradeRequestController(IUpgradeRequestService upgradeRequestService)
        {
            _upgradeRequestService = upgradeRequestService;
        }

        /// <summary>
        /// Get user info and current request status
        /// </summary>
        [HttpGet("my-info")]
        public async Task<IActionResult> GetMyInfo()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return Unauthorized();

            var userId = GetUserId();

            var response = await _upgradeRequestService.GetUserForUpgradeAsync(userId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Get user's request history
        /// </summary>
        [HttpGet("my-requests")]
        [Authorize(Roles = "Customer,Owner")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var response = await _upgradeRequestService.GetMyRequestsAsync(userId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Create upgrade request (Customer only)
        /// </summary>
        [HttpPost("create-request")]
        public async Task<IActionResult> CreateRequestAsync([FromBody] CreateUpgradeRequestDTO request)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var response = await _upgradeRequestService.CreateRequestAsync(userId, request.Address, request.TaxCode);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Cancel a pending request
        /// </summary>
        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var response = await _upgradeRequestService.CancelRequestAsync(userId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}