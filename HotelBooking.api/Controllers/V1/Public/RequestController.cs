using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelBooking.application.Services.Domains.RequestManagement;

namespace HotelBooking.api.Controllers.V1.Public
{
    /// <summary>
    /// Public Request Controller - endpoints dành cho khách hàng
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IUpgradeRequestService _upgradeRequestService;

        public RequestController(IUpgradeRequestService upgradeRequestService)
        {
            _upgradeRequestService = upgradeRequestService;
        }

        /// <summary>
        /// Lấy thông tin người dùng để hiển thị form nâng cấp
        /// </summary>
        [HttpGet("get-user-upgrade")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetUserForUpgrade()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return BadRequest("User identifier claim is missing.");

            var userId = int.Parse(claim.Value);
            var dto = await _upgradeRequestService.GetUserForUpgradeAsync(userId);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Tạo yêu cầu nâng cấp (Customer only)
        /// </summary>
        [HttpPost("create-request")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateRequestAsync([FromBody] CreateUpgradeRequestDTO request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
                return BadRequest("User identifier claim is missing.");

            var userId = int.Parse(claim.Value);
            var result = await _upgradeRequestService.CreateRequestAsync(userId, request.Address, request.TaxCode);
            if (result)
                return Ok(new { Message = "Request created successfully." });
            return BadRequest(new { Message = "Failed to create request." });
        }
    }
}