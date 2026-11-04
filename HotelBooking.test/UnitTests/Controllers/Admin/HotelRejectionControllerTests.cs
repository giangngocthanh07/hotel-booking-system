using System.Security.Claims;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using V1.Controllers;

namespace HotelBooking.test.UnitTests.Controllers.Admin;

[Trait("Requirement", "FR-ONB-017")]
// Verify the new JSON-body contract without requiring a database or a running API host.
// These direct controller tests do not exercise HTTP model binding or authentication middleware.
public class HotelRejectionControllerTests
{
    [Theory]
    [InlineData(StatusCodeResponse.Success, 200)]
    [InlineData(StatusCodeResponse.BadRequest, 400)]
    public async Task Reject_ForwardsBodyAndTrustedAdminIdentity(string status, int httpStatus)
    {
        // A strict setup detects dropped reasons, incorrect Admin identity, or lost cancellation tokens.
        using var cancellation = new CancellationTokenSource();
        var input = new RejectHotelRegistrationDTO { Reason = "Business license is incomplete." };
        var response = new ApiResponse<bool> { StatusCode = status };
        var service = new Mock<IAdminHotelApprovalRequestService>(MockBehavior.Strict);
        service.Setup(value => value.RejectRequestAsync(99, 42, input, cancellation.Token)).ReturnsAsync(response);
        var controller = CreateController(service.Object);
        var result = await controller.Reject(99, input, cancellation.Token);
        var objectResult = Assert.IsType<ObjectResult>(result);
        // Preserve the service response while mapping validation failures to HTTP 400.
        Assert.Equal(httpStatus, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);
        service.Verify(value => value.RejectRequestAsync(99, 42, input, cancellation.Token), Times.Once);
        service.VerifyNoOtherCalls();
    }

    private static AdminHotelApprovalRequestController CreateController(IAdminHotelApprovalRequestService service)
    {
        // Controller unit tests supply the trusted identity that authentication middleware normally creates.
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "42")], "test");
        return new AdminHotelApprovalRequestController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }
}
