using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HotelBooking.application.Services.Domains.Media;
using Moq;

namespace HotelBooking.test.UnitTests.Services.Media;

[Trait("Requirement", "FR-ONB-013")]
[Trait("Requirement", "FR-MED-004")]
public class BusinessLicenseUploaderTests
{
    [Theory]
    [InlineData("license.pdf")]
    [InlineData("license.JPG")]
    [InlineData("license.jpeg")]
    [InlineData("license.png")]
    public async Task Upload_PreservesOriginalBytesInPrivateRawAsset(string filename)
    {
        using var cancellation = new CancellationTokenSource();
        await using var content = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var sdk = new Mock<ICloudinaryUploadApi>(MockBehavior.Strict);
        RawUploadParams? sent = null;
        sdk.Setup(api => api.UploadAsync(It.IsAny<RawUploadParams>(), null, cancellation.Token))
            .Callback<RawUploadParams, string, CancellationToken?>((parameters, _, _) => sent = parameters)
            .ReturnsAsync(Uploaded());
        var result = await new BusinessLicenseUploader(sdk.Object).UploadAsync(
            new UploadFileDTO { FileName = filename, Content = content }, 7, cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(sent);
        AssertPrivateOriginal(sent, filename, content);
        sdk.VerifyAll();
    }

    private static void AssertPrivateOriginal(RawUploadParams sent, string filename, MemoryStream content)
    {
        Assert.Equal("private", sent.Type);
        Assert.Equal(ResourceType.Raw, sent.ResourceType);
        Assert.EndsWith(Path.GetExtension(filename).ToLowerInvariant(), sent.PublicId);
        Assert.StartsWith("user_7_business_license_", sent.PublicId);
        Assert.DoesNotContain("license.", sent.PublicId);
        Assert.Same(content, sent.File.Stream);
        Assert.Equal(0, content.Position);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, content.ToArray());
    }

    [Fact]
    public async Task ProviderError_DoesNotReturnReceiptMetadata()
    {
        var sdk = new Mock<ICloudinaryUploadApi>();
        sdk.Setup(api => api.UploadAsync(It.IsAny<RawUploadParams>(), null, default(CancellationToken)))
            .ReturnsAsync(new RawUploadResult { Error = new Error { Message = "Upload rejected." } });
        await using var content = new MemoryStream(new byte[] { 1 });
        var result = await new BusinessLicenseUploader(sdk.Object).UploadAsync(
            new UploadFileDTO { FileName = "license.pdf", Content = content }, 7, default);
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task ProviderCancellation_Propagates()
    {
        var sdk = new Mock<ICloudinaryUploadApi>();
        sdk.Setup(api => api.UploadAsync(It.IsAny<RawUploadParams>(), null, default(CancellationToken)))
            .ThrowsAsync(new OperationCanceledException());
        await using var content = new MemoryStream(new byte[] { 1 });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new BusinessLicenseUploader(sdk.Object)
            .UploadAsync(new UploadFileDTO { FileName = "license.pdf", Content = content }, 7, default));
    }

    [Theory]
    [InlineData("raw", "upload")]
    [InlineData("image", "private")]
    public async Task UnexpectedProviderMetadata_DoesNotReturnReceiptMetadata(string resource, string delivery)
    {
        var sdk = new Mock<ICloudinaryUploadApi>();
        var uploaded = Uploaded();
        uploaded.ResourceType = resource;
        uploaded.Type = delivery;
        sdk.Setup(api => api.UploadAsync(It.IsAny<RawUploadParams>(), null, default(CancellationToken)))
            .ReturnsAsync(uploaded);
        await using var content = new MemoryStream(new byte[] { 1 });
        var result = await new BusinessLicenseUploader(sdk.Object).UploadAsync(
            new UploadFileDTO { FileName = "license.pdf", Content = content }, 7, default);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task CancelledUpload_DoesNotCallProvider()
    {
        var sdk = new Mock<ICloudinaryUploadApi>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new BusinessLicenseUploader(sdk.Object)
            .UploadAsync(new UploadFileDTO(), 7, cancellation.Token));
        sdk.VerifyNoOtherCalls();
    }

    private static RawUploadResult Uploaded() => new()
    {
        PublicId = "owner/server-id.pdf", ResourceType = "raw", Type = "private",
        SecureUrl = new Uri("https://res.cloudinary.com/test/raw/private/owner/server-id.pdf"),
    };
}
