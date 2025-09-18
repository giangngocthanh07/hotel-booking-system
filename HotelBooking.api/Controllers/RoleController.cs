namespace HotelBooking.api.Controllers
{

    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("AddRole")]
        public async Task<ActionResult> AddRole([FromBody] RoleDTO newRole)
        {
            var response = await _roleService.AddAsync(newRole);
            if (response)
            {
                return Ok(RoleMessage.ROLE_ADD_SUCCESS);
            }
            return BadRequest(RoleMessage.ROLE_ADD_FAILED);
        }
    }
}