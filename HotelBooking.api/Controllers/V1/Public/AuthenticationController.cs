using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HotelBooking.application.DTOs.User;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.UserManagement;

namespace HotelBooking.api.Controllers.V1.Public
{
    /// <summary>
    /// Public Authentication Controller - Registration, login, user information management
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
        /// Get current user information (Requires authentication)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
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
        /// Register customer account (public)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDTO newCustomer)
        {
            var response = await _userService.Register.RegisterCustomer(newCustomer);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Login (public)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginRequest)
        {
            var response = await _userService.Login.LoginUser(loginRequest);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        /// <summary>
        /// Update current user profile (Requires authentication)
        /// </summary>
        [HttpPut("update-profile/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserProfileDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized();
            }

            // Security check: user can only update their own profile
            if (userId != id)
            {
                return Forbid();
            }

            var response = await _userService.UpdateProfileAsync(id, request);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

    }
}
