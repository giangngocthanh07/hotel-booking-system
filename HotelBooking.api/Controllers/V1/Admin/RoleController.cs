using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Services.Domains.AdminManagement;
// Note: RoleMessage được consolidate vào MessageResponse.AdminManagement.Role
// Dùng MessageResponse.AdminManagement.Role.* cho code mới hoặc RoleMessage untuk backward compatible

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
                return Ok();
            }
            return BadRequest();
        }
    }
}