using System.Security.Claims;
using HotelBooking.api.Controllers.V1.Owner;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Admin;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using V1.Controllers;

namespace HotelBooking.test.UnitTests.Controllers;

// Direct controller tests verify the identity boundary, not authentication middleware.
[Trait("Requirement", "FR-ONB-009")]
[Trait("Requirement", "FR-ONB-013")]
[Trait("Requirement", "FR-ONB-015")]
[Trait("Requirement", "FR-ONB-017")]
public class HotelRegistrationIdentityTests
{
    public static TheoryData<string?> InvalidClaims => new()
        { null, "", "invalid", "2147483648", "0", "-1" };

    [Theory]
    [MemberData(nameof(InvalidClaims))]
    public async Task InvalidOwnerIdentity_DoesNotCallServices(string? claim)
    {
        var service = new Mock<IHotelRegistrationService>(MockBehavior.Strict);
        var history = new Mock<IHotelRegistrationHistoryService>(MockBehavior.Strict);
        var controller = WithIdentity(new RequestController(service.Object, history.Object), claim);
        Assert.IsType<UnauthorizedResult>(await controller.HotelRegistration(new HotelRegistrationDTO()));
        Assert.IsType<UnauthorizedResult>(await controller.GetMyHotelRequests());
        Assert.IsType<UnauthorizedResult>(await controller.GetMyHistoryPage(new PagingRequest(), default));
        service.VerifyNoOtherCalls();
        history.VerifyNoOtherCalls();
    }

    [Theory]
    [MemberData(nameof(InvalidClaims))]
    public async Task InvalidAdminIdentity_DoesNotCallServices(string? claim)
    {
        var service = new Mock<IAdminHotelApprovalRequestService>(MockBehavior.Strict);
        var controller = WithIdentity(new AdminHotelApprovalRequestController(service.Object), claim);
        Assert.IsType<UnauthorizedResult>(await controller.Approve(99));
        Assert.IsType<UnauthorizedResult>(await controller.Reject(99, new RejectHotelRegistrationDTO()));
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidOwnerIdentity_ForwardsSubmissionAndHistory()
    {
        var input = new HotelRegistrationDTO();
        var created = new ApiResponse<HotelRegistrationDetailDTO> { StatusCode = StatusCodeResponse.Success };
        var history = new ApiResponse<List<HotelRegistrationDetailDTO>> { StatusCode = StatusCodeResponse.Success };
        var service = new Mock<IHotelRegistrationService>(MockBehavior.Strict);
        service.Setup(value => value.CreateRequestAsync(42, input)).ReturnsAsync(created);
        service.Setup(value => value.GetMyRequestsAsync(42)).ReturnsAsync(history);
        var controller = WithIdentity(new RequestController(service.Object, Mock.Of<IHotelRegistrationHistoryService>()), "42");
        Assert.Same(created, Assert.IsType<ObjectResult>(await controller.HotelRegistration(input)).Value);
        Assert.Same(history, Assert.IsType<ObjectResult>(await controller.GetMyHotelRequests()).Value);
        service.Verify(value => value.CreateRequestAsync(42, input), Times.Once);
        service.Verify(value => value.GetMyRequestsAsync(42), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidAdminIdentity_ForwardsApproval()
    {
        var response = new ApiResponse<bool> { StatusCode = StatusCodeResponse.Success };
        var service = new Mock<IAdminHotelApprovalRequestService>(MockBehavior.Strict);
        service.Setup(value => value.ApproveRequestAsync(99, 42)).ReturnsAsync(response);
        var controller = WithIdentity(new AdminHotelApprovalRequestController(service.Object), "42");
        Assert.Same(response, Assert.IsType<ObjectResult>(await controller.Approve(99)).Value);
        service.Verify(value => value.ApproveRequestAsync(99, 42), Times.Once);
        service.VerifyNoOtherCalls();
    }

    private static T WithIdentity<T>(T controller, string? claim) where T : ControllerBase
    {
        var claims = claim == null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, claim)];
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) }
        };
        return controller;
    }
}
