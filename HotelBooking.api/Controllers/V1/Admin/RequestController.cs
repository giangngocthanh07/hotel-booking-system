using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Services.Domains.RequestManagement;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin Requests Controller - Admin quản lý các đơn yêu cầu từ khách hàng
    /// </summary>
    [Route("api/v1/admin/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IUpgradeRequestService _upgradeRequestService;

        public RequestController(IUpgradeRequestService upgradeRequestService)
        {
            _upgradeRequestService = upgradeRequestService;
        }


        [HttpGet("get-all-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRequestsAsync([FromQuery] string? status)
        {
            var requests = await _upgradeRequestService.GetAllRequestAsync(status);
            return Ok(requests);
        }

        [HttpGet("request-detail/{requestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByRequestIdAsync(int requestId)
        {
            var request = await _upgradeRequestService.GetByRequestIdAsync(requestId);
            if (request == null) return NotFound();
            return Ok(request);
        }

        [HttpPost("approve/{requestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUpgradeAsync(int requestId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid");
            if (claim == null) return BadRequest("AdminId claim is missing.");

            var adminId = int.Parse(claim.Value);
            var success = await _upgradeRequestService.ApproveRequestAsync(requestId, adminId);
            if (!success) return BadRequest("Cannot approve upgrade request.");
            else
                return Ok("Approved upgrade request successfully.");
        }

        [HttpPost("reject/{requestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectUpgradeAsync(int requestId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("nameid");
            if (claim == null) return BadRequest("AdminId claim is missing.");

            var adminId = int.Parse(claim.Value);
            var success = await _upgradeRequestService.RejectRequestAsync(requestId, adminId);
            if (!success) return BadRequest("Cannot reject upgrade request.");
            else
                return Ok("Rejected upgrade request successfully.");
        }
    }
}
