using System.Security.Claims;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Public
{
    /// <summary>
    /// Authentication Controller - Đăng ký, đăng nhập, quản lý tài khoản
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        /// <summary>
        /// Đăng ký Admin (chỉ admin có quyền)
        /// </summary>
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDTO newAdmin)
        {
            var response = await _userService.RegisterAdmin(newAdmin);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Đăng ký Customer
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDTO newCustomer)
        {
            var response = await _userService.RegisterCustomer(newCustomer);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginRequest)
        {
            var response = await _userService.LoginUser(loginRequest);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
    }
}
