using HotelBooking.application.Services.Domains.Media.BusinessLicenses;
using HotelBooking.application.DTOs.Media.BusinessLicenses;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.Media.BusinessLicenses;

[Trait("Requirement", "FR-ONB-013")]
[Trait("Requirement", "FR-ONB-014")]
public class BusinessLicenseAccessTests
{
    [Theory]
    [InlineData(false, 7)]
    [InlineData(true, null)]
    public async Task Access_UsesTrustedOwnerScopeBeforeDownloading(bool admin, int? expectedOwner)
    {
        using var cancellation = new CancellationTokenSource();
        var uploads = new Mock<IBusinessLicenseUploadRepository>(MockBehavior.Strict);
        uploads.Setup(repo => repo.GetForRequestAsync(42, expectedOwner, cancellation.Token)).ReturnsAsync(Upload());
        var downloader = new Mock<IBusinessLicenseDownloader>(MockBehavior.Strict);
        downloader.Setup(provider => provider.DownloadAsync(It.IsAny<BusinessLicenseDownloadSource>(), cancellation.Token))
            .ReturnsAsync(ResponseFactory.Success("%PDF-1.7"u8.ToArray(), "Retrieved."));
        var result = await new BusinessLicenseAccessService(uploads.Object, downloader.Object)
            .DownloadAsync(new(42, 7, admin), cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("application/pdf", result.Content!.ContentType);
        Assert.Equal("business-license-42.pdf", result.Content.FileName);
        uploads.VerifyAll();
        downloader.VerifyAll();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("upload")]
    public async Task UnavailableOrLegacyDocument_DoesNotDownload(string? delivery)
    {
        var uploads = new Mock<IBusinessLicenseUploadRepository>();
        var row = delivery == null ? null : Upload();
        if (row != null) row.DeliveryType = delivery!;
        uploads.Setup(repo => repo.GetForRequestAsync(42, 7, default)).ReturnsAsync(row);
        var downloader = new Mock<IBusinessLicenseDownloader>(MockBehavior.Strict);
        var result = await new BusinessLicenseAccessService(uploads.Object, downloader.Object).DownloadAsync(new(42, 7, false));
        Assert.Equal(StatusCodeResponse.NotFound, result.StatusCode);
        Assert.Null(result.Content);
        downloader.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(42, 0)]
    public async Task InvalidIdentifiers_DoNotReadRegistry(int requestId, int userId)
    {
        var uploads = new Mock<IBusinessLicenseUploadRepository>(MockBehavior.Strict);
        var downloader = new Mock<IBusinessLicenseDownloader>(MockBehavior.Strict);
        var result = await new BusinessLicenseAccessService(uploads.Object, downloader.Object).DownloadAsync(new(requestId, userId, false));
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        uploads.VerifyNoOtherCalls();
        downloader.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true, 7)]
    [InlineData(false, 8)]
    public async Task InconsistentRegistryRecord_DoesNotDownload(bool deleted, int ownerId)
    {
        var row = Upload();
        row.IsDeleted = deleted;
        row.OwnerId = ownerId;
        var uploads = new Mock<IBusinessLicenseUploadRepository>();
        uploads.Setup(repo => repo.GetForRequestAsync(42, 7, default)).ReturnsAsync(row);
        var downloader = new Mock<IBusinessLicenseDownloader>(MockBehavior.Strict);
        var result = await new BusinessLicenseAccessService(uploads.Object, downloader.Object).DownloadAsync(new(42, 7, false));
        Assert.Equal(StatusCodeResponse.NotFound, result.StatusCode);
        downloader.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LookupCancellation_PropagatesWithoutProviderAccess()
    {
        var uploads = new Mock<IBusinessLicenseUploadRepository>();
        uploads.Setup(repo => repo.GetForRequestAsync(42, 7, It.IsAny<CancellationToken>())).ThrowsAsync(new OperationCanceledException());
        var downloader = new Mock<IBusinessLicenseDownloader>(MockBehavior.Strict);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new BusinessLicenseAccessService(uploads.Object, downloader.Object).DownloadAsync(new(42, 7, false)));
        downloader.VerifyNoOtherCalls();
    }

    private static BusinessLicenseUpload Upload() => new()
    {
        PublicId = "owner/license.pdf", ResourceType = "raw", DeliveryType = "private", ContentType = "application/pdf",
        SizeBytes = 8, OriginalFileName = "unsafe\r\nfilename.pdf", OwnerId = 7,
    };
}
