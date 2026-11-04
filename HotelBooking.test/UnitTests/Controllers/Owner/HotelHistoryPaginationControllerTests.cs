using System.Security.Claims;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.RequestManagement.Owner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using HotelBooking.api.Controllers.V1.Owner;

namespace HotelBooking.test.UnitTests.Controllers.Owner;

[Trait("Requirement", "FR-ONB-013")]
public class HotelHistoryPaginationControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task InvalidIdentity_DoesNotQueryPagedHistory(string? claim)
    {
        var history = new Mock<IHotelRegistrationHistoryService>(MockBehavior.Strict);
        var controller = Create(history.Object, claim);
        Assert.IsType<UnauthorizedResult>(await controller.GetMyHistoryPage(new PagingRequest(), default));
        history.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidIdentity_ForwardsClaimsOwnerPagingAndCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var paging = new PagingRequest { PageIndex = 2, PageSize = 5 };
        var history = new Mock<IHotelRegistrationHistoryService>(MockBehavior.Strict);
        var response = new ApiResponse<PagedResult<HotelRegistrationDetailDTO>> { StatusCode = "Success" };
        history.Setup(service => service.GetPageAsync(7, paging, cancellation.Token)).ReturnsAsync(response);
        var result = await Create(history.Object, "7").GetMyHistoryPage(paging, cancellation.Token);
        Assert.Same(response, Assert.IsType<ObjectResult>(result).Value);
        history.Verify(service => service.GetPageAsync(7, paging, cancellation.Token), Times.Once);
    }

    private static RequestController Create(IHotelRegistrationHistoryService history, string? claim)
    {
        // Unit-test identity forwarding; HTTP role middleware is outside this test's scope.
        var claims = claim == null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, claim)];
        return new RequestController(Mock.Of<IHotelRegistrationService>(), history)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) } }
        };
    }
}
