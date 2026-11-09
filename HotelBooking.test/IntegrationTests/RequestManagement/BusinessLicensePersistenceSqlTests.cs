using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Validators.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HotelBooking.Tests.Integration.RequestManagement;

[Trait("Requirement", "FR-ONB-009")]
[Trait("Requirement", "FR-ONB-013")]
[Trait("Category", "SqlIntegration")]
public class BusinessLicensePersistenceSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Fact]
    public async Task ValidUpload_ReturnsReceiptForDurableOwnerRecord()
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        await using var unit = new UnitOfWork(context);
        var provider = Provider();
        var service = new FileService(provider.Object, new BusinessLicenseUploadValidator(), NullLogger<FileService>.Instance,
            new BusinessLicenseUploadRepository(context), unit);
        await using var bytes = new MemoryStream("%PDF-1.7"u8.ToArray());
        var response = await service.UploadBusinessLicenseAsync(File(bytes), seed.OwnerId);
        Assert.Equal(StatusCodeResponse.Success, response.StatusCode);
        Assert.NotNull(response.Content!.UploadId);
        await using var verify = fixture.CreateContext();
        var stored = await verify.BusinessLicenseUploads.SingleAsync(row => row.Id == response.Content.UploadId);
        Assert.Equal(seed.OwnerId, stored.OwnerId);
        Assert.Equal("provider/real-metadata", stored.PublicId);
        Assert.Equal("https://example.com/license.pdf", stored.StoredUrl);
        Assert.Equal(response.Content.StoredFileName, stored.StoredUrl);
        Assert.Equal(("image", "upload", "application/pdf", 8L, false),
            (stored.ResourceType, stored.DeliveryType, stored.ContentType, stored.SizeBytes, stored.IsDeleted));
        Assert.Empty(await verify.Hotels.Where(hotel => hotel.Name == seed.Name).ToListAsync());
    }

    [Fact]
    public async Task MissingOwner_DatabaseFailureDoesNotReturnReceipt()
    {
        await using var context = fixture.CreateContext();
        await using var unit = new UnitOfWork(context);
        var service = new FileService(Provider().Object, new BusinessLicenseUploadValidator(), NullLogger<FileService>.Instance,
            new BusinessLicenseUploadRepository(context), unit);
        await using var bytes = new MemoryStream("%PDF-1.7"u8.ToArray());
        var result = await service.UploadBusinessLicenseAsync(File(bytes), int.MaxValue);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Null(result.Content);
        await using var verify = fixture.CreateContext();
        Assert.False(await verify.BusinessLicenseUploads.AnyAsync(row => row.OwnerId == int.MaxValue));
    }

    private static Mock<IBusinessLicenseUploader> Provider()
    {
        var provider = new Mock<IBusinessLicenseUploader>(MockBehavior.Strict);
        provider.Setup(value => value.UploadAsync(It.IsAny<UploadFileDTO>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<BusinessLicenseProviderUpload> { StatusCode = StatusCodeResponse.Success,
                Content = new BusinessLicenseProviderUpload("provider/real-metadata", "image", "upload", "https://example.com/license.pdf") });
        return provider;
    }

    private static UploadFileDTO File(Stream content) =>
        new() { FileName = "license.pdf", ContentType = "application/pdf", Size = 8, Content = content };
}
