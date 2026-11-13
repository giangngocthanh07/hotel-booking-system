using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HotelBooking.test.UnitTests.Services.Media.HotelImages;

[Trait("Requirement", "FR-MED-003")]
[Trait("Requirement", "FR-MED-004")]
public class HotelPhotoUploadTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HotelUpload_UsesPublicImageParametersWithoutCropping(bool cover)
    {
        using var cancellation = new CancellationTokenSource();
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        ImageUploadParams? sent = null;
        var sdk = new Mock<ICloudinaryUploadApi>(MockBehavior.Strict);
        sdk.Setup(api => api.UploadAsync(It.IsAny<ImageUploadParams>(), cancellation.Token))
            .Callback<ImageUploadParams, CancellationToken?>((parameters, _) => sent = parameters)
            .ReturnsAsync(new ImageUploadResult { ResourceType = "image", Type = "upload", SecureUrl = new Uri("https://example.com/new.jpg") });
        var result = await new PhotoService(sdk.Object, NullLogger<PhotoService>.Instance)
            .UploadHotelImageAsync(new UploadFileDTO { FileName = "untrusted.jpg", Content = content }, 7, 42, cover, cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(sent);
        Assert.Null(sent.Transformation);
        Assert.Equal("upload", sent.Type);
        Assert.StartsWith("hotel_42_", sent.PublicId);
        Assert.DoesNotContain("untrusted", sent.PublicId);
        Assert.Same(content, sent.File.Stream);
        sdk.VerifyAll();
    }
}
