using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace HotelBooking.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get-user-by-id")]
        public async Task<ActionResult> GetUserByIdAsync(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegisterAdminDTO newAdmin)
        {
            var res = await _userService.RegisterAdmin(newAdmin);
            if (res.IsSuccess)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res.Message ?? MessageRegister.REGISTER_FAIL);
            }
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomerAsync([FromBody] RegisterCustomerDTO newCustomer)
        {
            var res = await _userService.RegisterCustomer(newCustomer);
            if (res.IsSuccess)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res.Message ?? MessageRegister.REGISTER_FAIL);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUserAsync([FromBody] LoginUserDTO userLogin)
        {
            var res = await _userService.LoginUser(userLogin);

            if (res == null)
            {
                return StatusCode(500, MessageLogin.ERROR_IN_SERVER);
            }

            if (res.Message == MessageLogin.USER_NOT_FOUND)
            {
                return NotFound(res.Message);
            }
            else if (res.Message == MessageLogin.PASSWORD_INCORRECT)
            {
                return BadRequest(res.Message);
            }
            else if (res.Message == MessageLogin.ERROR_IN_SERVER)
            {
                return StatusCode(500, res.Message);
            }
            else
            {
                return Ok(res); // lúc này res chứa cả Token, UserInfo, Roles, Message
            }
        }

        // [HttpPost("upgrade-request")]
        // [Authorize(Roles = "Customer")]
        // public async Task<ActionResult> RequestUpgradeAsync()
        // {
        //     var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        //     if (claim == null || string.IsNullOrEmpty(claim.Value))
        //     {
        //         return BadRequest("User identifier claim is missing.");
        //     }

        //     var userId = int.Parse(claim.Value);
        //     var success = await _userService.RequestUpgradeToOwnerAsync(userId);
        //     if (!success) return BadRequest("Cannot process upgrade request.");
        //     else
        //         return Ok("Sent upgrade request successfully. Please wait for admin approval.");
        // }

        [HttpPost("upgrade-approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApproveUpgradeAsync(int requestId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return BadRequest("User identifier claim is missing.");
            }
            var adminId = int.Parse(claim.Value);
            var success = await _userService.ApproveUpgradeToOwnerAsync(requestId, adminId);
            if (!success) return BadRequest("Cannot approve upgrade request.");
            else
                return Ok("Approved upgrade request successfully.");
        }
    }
}