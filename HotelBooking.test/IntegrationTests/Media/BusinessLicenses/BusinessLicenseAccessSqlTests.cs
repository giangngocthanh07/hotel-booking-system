using HotelBooking.application.Services.Domains.Media.BusinessLicenses;
using HotelBooking.application.DTOs.Media.BusinessLicenses;
using HotelBooking.Tests.Integration.RequestManagement;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HotelBooking.Tests.Integration.Media.BusinessLicenses;

[Trait("Requirement", "FR-ONB-013")]
[Trait("Requirement", "FR-ONB-014")]
[Trait("Category", "SqlIntegration")]
public class BusinessLicenseAccessSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Theory]
    [InlineData("owner", true)]
    [InlineData("admin", true)]
    [InlineData("foreign", false)]
    [InlineData("deleted", false)]
    [InlineData("legacy", false)]
    [InlineData("unknown", false)]
    public async Task Download_ResolvesOnlyAuthorizedLinkedPrivateUpload(string scenario, bool allowed)
    {
        var seed = await SeedAsync(scenario);
        await using var context = fixture.CreateContext();
        var downloader = new Mock<IBusinessLicenseDownloader>(MockBehavior.Strict);
        if (allowed) downloader.Setup(provider => provider.DownloadAsync(It.IsAny<BusinessLicenseDownloadSource>(), default))
            .ReturnsAsync(ResponseFactory.Success("%PDF-1.7"u8.ToArray(), "Retrieved."));
        var service = new BusinessLicenseAccessService(new BusinessLicenseUploadRepository(context), downloader.Object);
        var userId = scenario == "foreign" ? seed.OtherAdminId : scenario == "admin" ? seed.AdminId : seed.OwnerId;
        var requestId = scenario == "unknown" ? int.MaxValue : seed.RequestId;
        var result = await service.DownloadAsync(new(requestId, userId, scenario == "admin"));
        Assert.Equal(allowed ? StatusCodeResponse.Success : StatusCodeResponse.NotFound, result.StatusCode);
        if (allowed) downloader.VerifyAll();
        else { Assert.Null(result.Content); downloader.VerifyNoOtherCalls(); }
    }

    private async Task<HotelRequestSeed> SeedAsync(string scenario)
    {
        var seed = await fixture.SeedAsync();
        if (scenario == "legacy") return seed;
        await using var context = fixture.CreateContext();
        var upload = new BusinessLicenseUpload
        {
            Id = Guid.NewGuid(), OwnerId = seed.OwnerId, PublicId = $"private/{Guid.NewGuid():N}.pdf",
            ResourceType = "raw", DeliveryType = "private", StoredUrl = "https://example.com/not-fetched.pdf",
            OriginalFileName = "license.pdf", ContentType = "application/pdf", SizeBytes = 8,
            CreatedAt = DateTime.UtcNow, IsDeleted = scenario == "deleted",
        };
        context.BusinessLicenseUploads.Add(upload);
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        request.BusinessLicenseUploadId = upload.Id;
        await context.SaveChangesAsync();
        return seed;
    }
}
