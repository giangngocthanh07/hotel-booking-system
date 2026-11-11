using System.Security.Claims;
using HotelBooking.api.Controllers.V1.Owner;
using HotelBooking.application.DTOs.Hotel.Owner;
using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Owner;

[Trait("Requirement", "FR-CAT-001")]
[Trait("Requirement", "FR-CAT-003")]
public class OwnerHotelControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public async Task InvalidIdentity_DoesNotCallServices(string? claim)
    {
        var service = new Mock<IOwnerHotelService>(MockBehavior.Strict);
        var controller = Controller(service.Object, claim);
        Assert.IsType<UnauthorizedResult>(await controller.GetPage(new()));
        Assert.IsType<UnauthorizedResult>(await controller.GetById(42));
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Page_ForwardsAuthenticatedIdentityAndCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var paging = new PagingRequest();
        var response = ResponseFactory.Success(new PagedResult<OwnerHotelSummaryDTO>([], 0, 1, 10), "OK");
        var service = new Mock<IOwnerHotelService>(MockBehavior.Strict);
        service.Setup(value => value.GetPageAsync(7, paging, cancellation.Token)).ReturnsAsync(response);
        var result = Assert.IsType<ObjectResult>(await Controller(service.Object, "7").GetPage(paging, cancellation.Token));
        Assert.Equal(200, result.StatusCode);
        Assert.Same(response, result.Value);
        service.VerifyAll();
    }

    [Fact]
    public async Task ForeignDetails_MapsNotFound()
    {
        using var cancellation = new CancellationTokenSource();
        var service = new Mock<IOwnerHotelService>(MockBehavior.Strict);
        service.Setup(value => value.GetByIdAsync(7, 42, cancellation.Token))
            .ReturnsAsync(ResponseFactory.Failure<OwnerHotelDetailsDTO>(StatusCodeResponse.NotFound, "Hotel not found."));
        Assert.Equal(404, Assert.IsType<ObjectResult>(await Controller(service.Object, "7").GetById(42, cancellation.Token)).StatusCode);
        service.VerifyAll();
    }

    [Fact]
    public void Controller_RequiresOwnerRole()
    {
        var attribute = Assert.Single(typeof(HotelController).GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Equal("Owner", ((AuthorizeAttribute)attribute).Roles);
    }

    private static HotelController Controller(IOwnerHotelService service, string? id)
    {
        var claims = id == null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, id)];
        return new HotelController(service) { ControllerContext = new ControllerContext
        { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) } } };
    }
}
