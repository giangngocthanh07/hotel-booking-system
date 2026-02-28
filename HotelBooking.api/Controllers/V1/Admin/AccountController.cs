using System.Security.Claims;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin Account Controller - Quản lý tài khoản, Đăng ký Admin
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
        /// Lấy thông tin người dùng hiện tại (Admin)
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
        /// Lấy thông tin người dùng theo ID (Admin)
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var response = await _userService.GetByIdAsync(userId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Đăng ký tài khoản Admin (Yêu cầu Admin)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDTO newAdmin)
        {
            var res = await _userService.RegisterAdmin(newAdmin);
            return ApiResponseHandlerHelper.HandleResponse(res);
        }

        /// <summary>
        /// Phé duyệt yêu cầu nâng cấp thành chủ khách sạn
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