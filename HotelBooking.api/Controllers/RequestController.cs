using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using HotelBooking.api.Models;

namespace HotelBooking.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IUpgradeRequestService _upgradeRequestService;
        public RequestController(IUpgradeRequestService upgradeRequestService)
        {
            _upgradeRequestService = upgradeRequestService;
        }

        [HttpGet("get-user-upgrade")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetUserForUpgrade()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var dto = await _upgradeRequestService.GetUserForUpgradeAsync(userId);

            if (dto == null) return NotFound();

            return Ok(dto);
        }

        [HttpPost("create-request")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateRequestAsync([FromBody] CreateUpgradeRequestDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _upgradeRequestService.CreateRequestAsync(userId, request.Address, request.TaxCode);
            if (result)
            {
                return Ok(new { Message = "Request created successfully." });
            }
            return BadRequest(new { Message = "Failed to create request." });
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