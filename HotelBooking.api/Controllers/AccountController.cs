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
        IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register-admin")]
        public async Task<ActionResult> RegisterAdminAsync([FromBody] RegisterAdminDTO newAdmin)
        {

            var res = await _userService.RegisterAdmin(newAdmin);
            if (res)
            {
                return Ok(MessageRegister.REGISTER_SUCCESS);
            }
            else
            {
                return BadRequest(MessageRegister.REGISTER_FAIL);
            }
        }

        [HttpPost("register-customer")]
        public async Task<ActionResult> RegisterCustomerAsync([FromBody] RegisterCustomerDTO newCustomer)
        {

            var res = await _userService.RegisterCustomer(newCustomer);
            if (res)
            {
                return Ok(MessageRegister.REGISTER_SUCCESS);
            }
            else
            {
                return BadRequest(MessageRegister.REGISTER_FAIL);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUserAsync([FromBody] LoginUserDTO userLogin)
        {
            var res = await _userService.LoginUser(userLogin);

            if (res == MessageLogin.USER_NOT_FOUND)
            {
                return NotFound(res);
            }
            else if (res == MessageLogin.PASSWORD_INCORRECT)
            {
                return BadRequest(res);
            }
            else if (res == MessageLogin.ERROR_IN_SERVER)
            {
                return StatusCode(500, res);
            }
            else
            {
                return Ok(res);
            }
        }

        [HttpPost("upgrade-request")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult> RequestUpgradeAsync()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return BadRequest("User identifier claim is missing.");
            }

            var userId = int.Parse(claim.Value);
            var success = await _userService.RequestUpgradeToOwnerAsync(userId);
            if (!success) return BadRequest("Cannot process upgrade request.");
            else
                return Ok("Sent upgrade request successfully. Please wait for admin approval.");
        }

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