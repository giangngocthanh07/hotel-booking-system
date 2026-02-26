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
            if (userId == 0) return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID (Admin)
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
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
                return BadRequest("User identifier claim is missing.");
            }

            var adminId = int.Parse(claim.Value);
            var success = await _userService.ApproveUpgradeToOwnerAsync(requestId, adminId);
            if (!success) return BadRequest("Cannot approve upgrade request.");
            return Ok("Approved upgrade request successfully.");
        }
    }
}