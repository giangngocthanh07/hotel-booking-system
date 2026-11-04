using System.Security.Claims;
using HotelBooking.api.Controllers.V1.Admin;
using HotelBooking.application.DTOs.Request.UpgradeRequest;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Admin;

[Trait("Requirement", "FR-ONB-008")]
// Verify the Admin-only endpoint keeps the reason and authenticated identity intact.
public class UpgradeRejectionControllerTests
{
    [Fact]
    public async Task Reject_ForwardsReasonAndTrustedAdminIdentity()
    {
        using var cancellation = new CancellationTokenSource();
        var input = new RejectUpgradeRequestDTO { Reason = "Business documents are incomplete." };
        var response = new ApiResponse<bool> { StatusCode = StatusCodeResponse.Success };
        var service = new Mock<IAdminUpgradeRequestService>(MockBehavior.Strict);
        service.Setup(value => value.RejectRequestAsync(99, 42, input, cancellation.Token)).ReturnsAsync(response);

        var result = await CreateController(service.Object).Reject(99, input, cancellation.Token);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
        service.VerifyAll();
    }

    private static AdminUpgradeRequestController CreateController(IAdminUpgradeRequestService service)
    {
        // The controller trusts the identity produced by authentication, never an ID from the request body.
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "42")], "test");
        return new AdminUpgradeRequestController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }
}
