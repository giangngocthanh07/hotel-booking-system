using System.Security.Claims;
using HotelBooking.api.Controllers.V1.Owner.Images;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Owner.Images;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-MED-005")]
public class HotelImageDeleteControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    public async Task InvalidIdentity_DoesNotDelete(string? identity)
    {
        var remover = new Mock<IOwnerHotelImageDeleteService>(MockBehavior.Strict);
        Assert.IsType<UnauthorizedResult>(await Controller(identity).DeleteGallery(42, 9, remover.Object));
        remover.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_ForwardsAuthenticatedOwnerSelectedImageAndCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var remover = new Mock<IOwnerHotelImageDeleteService>(MockBehavior.Strict);
        remover.Setup(service => service.DeleteAsync(7, 42, 9, cancellation.Token)).ReturnsAsync(ResponseFactory.Success(true, "Removed."));
        var result = await Controller("7").DeleteGallery(42, 9, remover.Object, cancellation.Token);
        Assert.Equal(200, Assert.IsType<ObjectResult>(result).StatusCode);
        remover.VerifyAll();
    }

    private static HotelImagesController Controller(string? identity)
    {
        var claims = identity == null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, identity)];
        return new HotelImagesController(Mock.Of<IOwnerHotelImageReadService>()) { ControllerContext = new ControllerContext
        { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) } } };
    }
}
