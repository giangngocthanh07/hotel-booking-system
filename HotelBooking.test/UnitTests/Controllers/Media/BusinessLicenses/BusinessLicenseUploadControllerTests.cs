using HotelBooking.api.Controllers.V1.Media.BusinessLicenses;
using HotelBooking.application.Services.Domains.Media.BusinessLicenses;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers.Media.BusinessLicenses;

[Trait("Requirement", "FR-ONB-011")]
[Trait("Requirement", "FR-MED-003")]
public class BusinessLicenseUploadControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("bad")]
    [InlineData("2147483648")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task InvalidIdentity_ReturnsUnauthorizedWithoutService(string? claim)
    {
        var service = new Mock<IFileService>(MockBehavior.Strict);
        Assert.IsType<UnauthorizedResult>(await Controller(service.Object, claim).UploadBusinessLicense(null));
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingFile_ReturnsStructuredBadRequest()
    {
        var service = new Mock<IFileService>(MockBehavior.Strict);
        var result = Assert.IsType<ObjectResult>(await Controller(service.Object, "7").UploadBusinessLicense(null));
        Assert.Equal(400, result.StatusCode);
        Assert.Equal(StatusCodeResponse.BadRequest, Assert.IsType<ApiResponse<UploadResultDTO>>(result.Value).StatusCode);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForwardsMimeBytesAndCancellation_AndDisposesOpenedStream()
    {
        var stream = new MemoryStream("%PDF-1.7"u8.ToArray());
        var file = new Mock<IFormFile>();
        file.SetupGet(value => value.FileName).Returns("license.pdf");
        file.SetupGet(value => value.ContentType).Returns("application/pdf");
        file.SetupGet(value => value.Length).Returns(stream.Length);
        file.Setup(value => value.OpenReadStream()).Returns(stream);
        using var cancellation = new CancellationTokenSource();
        var response = new ApiResponse<UploadResultDTO> { StatusCode = StatusCodeResponse.Success };
        var service = new Mock<IFileService>(MockBehavior.Strict);
        service.Setup(value => value.UploadBusinessLicenseAsync(It.Is<UploadFileDTO>(upload =>
            upload.Content == stream && upload.ContentType == "application/pdf" && upload.Size == 8 && upload.FileName == "license.pdf"), 7, cancellation.Token))
            .ReturnsAsync(response);
        var result = await Controller(service.Object, "7").UploadBusinessLicense(file.Object, cancellation.Token);
        Assert.Same(response, Assert.IsType<ObjectResult>(result).Value);
        Assert.False(stream.CanRead);
        service.VerifyAll();
    }

    private static FileController Controller(IFileService service, string? claim)
    {
        var claims = claim == null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, claim)];
        return new FileController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
                { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) } },
        };
    }
}
