using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using HotelBooking.application.DTOs.User;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.UserManagement;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin Account Controller - Account Management, Admin Registration
    /// </summary>
    [Route("api/v1/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Tags("Admin - Account Management")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get current user information (Admin)
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return ApiResponseHandlerHelper.HandleResponse(
                    ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.Unauthorized, MessageResponse.Common.BAD_REQUEST));
            }

            var response = await _userService.GetByIdAsync(userId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Get user information by ID (Admin)
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var response = await _userService.GetByIdAsync(userId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Register Admin account (Requires Admin)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDTO newAdmin)
        {
            var res = await _userService.RegisterAdmin(newAdmin);
            return ApiResponseHandlerHelper.HandleResponse(res);
        }

        /// <summary>
        /// Approve upgrade request to hotel owner
        /// </summary>
        [HttpPost("{requestId}/upgrade-approve")]
        public async Task<IActionResult> ApproveUpgradeAsync(int requestId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return ApiResponseHandlerHelper.HandleResponse(
                    ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.Common.BAD_REQUEST));
            }

            var adminId = int.Parse(claim.Value);
            var response = await _userService.ApproveUpgradeToOwnerAsync(requestId, adminId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
    }
}