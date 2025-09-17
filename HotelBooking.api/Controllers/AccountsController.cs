using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using HotelBooking.api.Models;

namespace HotelBooking.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        // private readonly IUserService _userService;
        public AccountsController()
        {
            // _userService = userService;
        }

        [HttpGet("")]
        public async Task<ActionResult> GetTModels()
        {
            
            return Ok("ok");
        }
    }
}