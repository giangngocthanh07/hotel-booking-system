using System.Security.Claims;
using HotelBooking.api.Controllers.V1.Owner.Images;
using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Owner.Images;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-MED-001")]
[Trait("Requirement", "FR-MED-002")]
public class HotelImageUploadControllerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MultipartUpload_ForwardsMetadataIdentityAndCancellationAndDisposesStream(bool cover)
    {
        using var cancellation = new CancellationTokenSource();
        using var stream = new MemoryStream(new byte[] { 255, 216, 255 });
        var file = new FormFile(stream, 0, 3, "file", "hotel.jpg") { Headers = new HeaderDictionary(), ContentType = "image/jpeg" };
        var uploader = new Mock<IOwnerHotelImageUploadService>(MockBehavior.Strict);
        Stream? opened = null;
        uploader.Setup(service => service.UploadAsync(7, 42, It.Is<OwnerHotelImageFileDTO>(input =>
            input.FileName == "hotel.jpg" && input.Size == 3 && input.ContentType == "image/jpeg" && input.Content.CanRead), cover, cancellation.Token))
            .Callback<int, int, OwnerHotelImageFileDTO?, bool, CancellationToken>((_, _, input, _, _) => opened = input!.Content)
            .ReturnsAsync(ResponseFactory.Success(new OwnerHotelImageUploadDTO { HotelId = 42 }, "Uploaded."));
        var controller = Controller("7");
        var action = cover ? await controller.UploadCover(42, file, uploader.Object, cancellation.Token)
            : await controller.UploadGallery(42, file, uploader.Object, cancellation.Token);
        Assert.Equal(200, Assert.IsType<ObjectResult>(action).StatusCode);
        uploader.VerifyAll();
        Assert.NotNull(opened);
        Assert.Throws<ObjectDisposedException>(() => opened.ReadByte());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    public async Task InvalidIdentity_DoesNotUpload(string? id)
    {
        var uploader = new Mock<IOwnerHotelImageUploadService>(MockBehavior.Strict);
        Assert.IsType<UnauthorizedResult>(await Controller(id).UploadCover(42, null, uploader.Object));
        Assert.IsType<UnauthorizedResult>(await Controller(id).UploadGallery(42, null, uploader.Object));
        uploader.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingFile_ReturnsBadRequestWithoutServiceAccess()
    {
        var uploader = new Mock<IOwnerHotelImageUploadService>(MockBehavior.Strict);
        Assert.Equal(400, Assert.IsType<ObjectResult>(await Controller("7").UploadGallery(42, null, uploader.Object)).StatusCode);
        uploader.VerifyNoOtherCalls();
    }

    private static HotelImagesController Controller(string? id)
    {
        var claims = id == null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, id)];
        return new HotelImagesController(Mock.Of<IOwnerHotelImageReadService>()) { ControllerContext = new ControllerContext
        { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) } } };
    }
}
