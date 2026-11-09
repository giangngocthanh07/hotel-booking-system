using System.Security.Claims;
using HotelBooking.api.Controllers.V1;
using HotelBooking.application.Services.Domains.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelBooking.test.UnitTests.Controllers;

[Trait("Requirement", "FR-ONB-013")]
[Trait("Requirement", "FR-ONB-014")]
public class BusinessLicenseDocumentControllerTests
{
    [Theory]
    [InlineData("Owner", "7", false)]
    [InlineData("Admin", "9", true)]
    public async Task AuthorizedIdentity_ReturnsFileWithPrivateHeaders(string role, string id, bool admin)
    {
        using var cancellation = new CancellationTokenSource();
        var service = new Mock<IBusinessLicenseAccessService>(MockBehavior.Strict);
        service.Setup(value => value.DownloadAsync(new BusinessLicenseAccessRequest(42, int.Parse(id), admin), cancellation.Token))
            .ReturnsAsync(ResponseFactory.Success(new BusinessLicenseDocument([1, 2], "application/pdf", "business-license-42.pdf"), "Retrieved."));
        var controller = Controller(service.Object, role, id);
        var result = Assert.IsType<FileContentResult>(await controller.Download(42, cancellation.Token));
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("business-license-42.pdf", result.FileDownloadName);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
        Assert.Equal("nosniff", controller.Response.Headers.XContentTypeOptions.ToString());
        service.VerifyAll();
    }

    [Theory]
    [InlineData("Owner", null)]
    [InlineData("Owner", "invalid")]
    [InlineData("Admin", "0")]
    [InlineData("Customer", "7")]
    [InlineData(null, null)]
    public async Task InvalidIdentityOrRole_DoesNotCallService(string? role, string? id)
    {
        var service = new Mock<IBusinessLicenseAccessService>(MockBehavior.Strict);
        var result = await Controller(service.Object, role, id).Download(42, default);
        Assert.True(result is UnauthorizedResult or ForbidResult);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public void Endpoint_RequiresOwnerOrAdminAuthentication()
    {
        var attribute = Assert.Single(typeof(BusinessLicenseDocumentController).GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Equal("Owner,Admin", ((AuthorizeAttribute)attribute).Roles);
    }

    [Theory]
    [InlineData(StatusCodeResponse.NotFound, 404)]
    [InlineData(StatusCodeResponse.Error, 500)]
    public async Task UnavailableDocument_PreservesFailureStatusAndNoStore(string status, int httpStatus)
    {
        var service = new Mock<IBusinessLicenseAccessService>();
        service.Setup(value => value.DownloadAsync(new BusinessLicenseAccessRequest(42, 7, false), default))
            .ReturnsAsync(ResponseFactory.Failure<BusinessLicenseDocument>(status, "Business license is unavailable."));
        var controller = Controller(service.Object, "Owner", "7");
        Assert.Equal(httpStatus, Assert.IsType<ObjectResult>(await controller.Download(42, default)).StatusCode);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
    }

    private static BusinessLicenseDocumentController Controller(IBusinessLicenseAccessService service, string? role, string? id)
    {
        var claims = new List<Claim>();
        if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
        if (id != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, id));
        return new BusinessLicenseDocumentController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
            { User = new ClaimsPrincipal(new ClaimsIdentity(claims, role == null ? null : "test")) } },
        };
    }
}
