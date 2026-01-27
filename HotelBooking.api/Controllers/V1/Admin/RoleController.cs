using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Services.Domains.AdminManagement;

namespace HotelBooking.api.Controllers.V1.Admin
{

    /// <summary>
    /// Admin Roles Controller - Quản lý vai trò trong hệ thống
    /// </summary>

    [Route("api/v1/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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