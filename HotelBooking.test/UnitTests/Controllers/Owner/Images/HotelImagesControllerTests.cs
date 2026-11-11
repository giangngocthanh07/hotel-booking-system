using System.Security.Claims;
using HotelBooking.api.Controllers.V1.Owner.Images;
using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Owner.Images;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-MED-005")]
public class HotelImagesControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public async Task InvalidClaim_DoesNotCallService(string? id)
    {
        var service = new Mock<IOwnerHotelImageReadService>(MockBehavior.Strict);
        Assert.IsType<UnauthorizedResult>(await Controller(service.Object, id).Get(42));
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_ForwardsTrustedOwnerAndCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var service = new Mock<IOwnerHotelImageReadService>(MockBehavior.Strict);
        var response = ResponseFactory.Success(new OwnerHotelMediaDTO { HotelId = 42 }, "OK");
        service.Setup(value => value.GetAsync(7, 42, cancellation.Token)).ReturnsAsync(response);
        var result = Assert.IsType<ObjectResult>(await Controller(service.Object, "7").Get(42, cancellation.Token));
        Assert.Equal(200, result.StatusCode);
        Assert.Same(response, result.Value);
        service.VerifyAll();
    }

    [Fact]
    public void Endpoint_RequiresOwnerRole()
    {
        var attribute = Assert.Single(typeof(HotelImagesController).GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Equal("Owner", ((AuthorizeAttribute)attribute).Roles);
    }

    private static HotelImagesController Controller(IOwnerHotelImageReadService service, string? id)
    {
        var claims = id == null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, id)];
        return new HotelImagesController(service) { ControllerContext = new ControllerContext
        { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) } } };
    }
}
