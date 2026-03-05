using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.Helpers;
// Note: RoleMessage has been consolidated into MessageResponse.AdminManagement.Role
// Use MessageResponse.AdminManagement.Role.* for new code or RoleMessage for backward compatibility

namespace HotelBooking.api.Controllers.V1.Admin
{

    /// <summary>
    /// Admin Roles Controller - Role management in the system
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
        public async Task<IActionResult> AddRole([FromBody] RoleDTO newRole)
        {
            var response = await _roleService.AddAsync(newRole);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
    }
}