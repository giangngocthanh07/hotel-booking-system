using HotelBooking.application.Validators.Media.BusinessLicenses;
using HotelBooking.application.Services.Domains.Media.BusinessLicenses;
using HotelBooking.application.DTOs.Media.BusinessLicenses;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HotelBooking.test.UnitTests.Services.Media.BusinessLicenses;

[Trait("Requirement", "FR-ONB-009")]
[Trait("Requirement", "FR-ONB-013")]
public class BusinessLicensePersistenceTests
{
    private readonly Mock<IBusinessLicenseUploader> _provider = new(MockBehavior.Strict);
    private readonly Mock<IBusinessLicenseUploadRepository> _uploads = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unit = new(MockBehavior.Strict);
    private FileService Service => new(_provider.Object, new BusinessLicenseUploadValidator(),
        NullLogger<FileService>.Instance, _uploads.Object, _unit.Object);

    [Fact]
    public async Task Success_PersistsTrustedMetadataAndReturnsIdOnlyAfterSave()
    {
        using var cancellation = new CancellationTokenSource();
        await using var content = new MemoryStream("%PDF-1.7"u8.ToArray());
        BusinessLicenseUpload? row = null;
        SetupProvider();
        _uploads.Setup(repo => repo.AddAsync(It.IsAny<BusinessLicenseUpload>(), cancellation.Token))
            .Callback<BusinessLicenseUpload, CancellationToken>((value, _) => row = value).Returns(Task.CompletedTask);
        _unit.Setup(unit => unit.TrySaveChangesAsync(cancellation.Token)).ReturnsAsync(new SaveChangesResult(1, false));
        var before = DateTime.UtcNow;
        var response = await Service.UploadBusinessLicenseAsync(File(content), 7, cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, response.StatusCode);
        Assert.NotNull(row);
        Assert.NotEqual(Guid.Empty, row.Id);
        Assert.Equal(row.Id, response.Content!.UploadId);
        AssertMetadata(row, before);
        Assert.True(string.IsNullOrEmpty(response.Content.StoredFileName));
        _uploads.Verify(repo => repo.AddAsync(It.IsAny<BusinessLicenseUpload>(), cancellation.Token), Times.Once);
        _unit.Verify(unit => unit.TrySaveChangesAsync(cancellation.Token), Times.Once);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    public async Task PersistenceFailure_DoesNotReturnUsableReceipt(int rows, bool conflict)
    {
        await using var content = new MemoryStream("%PDF-1.7"u8.ToArray());
        SetupProvider();
        _uploads.Setup(repo => repo.AddAsync(It.IsAny<BusinessLicenseUpload>(), default)).Returns(Task.CompletedTask);
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(rows, conflict));
        var response = await Service.UploadBusinessLicenseAsync(File(content), 7);
        Assert.Equal(StatusCodeResponse.Error, response.StatusCode);
        Assert.Null(response.Content);
        _uploads.VerifyAll();
        _unit.VerifyAll();
    }

    [Fact]
    public async Task ProviderFailure_DoesNotWriteRegistryOrReceipt()
    {
        await using var content = new MemoryStream("%PDF-1.7"u8.ToArray());
        _provider.Setup(provider => provider.UploadAsync(It.IsAny<UploadFileDTO>(), 7, default))
            .ReturnsAsync(new ApiResponse<BusinessLicenseProviderUpload> { StatusCode = StatusCodeResponse.BadRequest, Message = "Provider rejected file." });
        var response = await Service.UploadBusinessLicenseAsync(File(content), 7);
        Assert.Equal(StatusCodeResponse.BadRequest, response.StatusCode);
        Assert.Null(response.Content);
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingProviderMetadata_DoesNotWriteRegistryOrReceipt()
    {
        await using var content = new MemoryStream("%PDF-1.7"u8.ToArray());
        _provider.Setup(provider => provider.UploadAsync(It.IsAny<UploadFileDTO>(), 7, default))
            .ReturnsAsync(new ApiResponse<BusinessLicenseProviderUpload> { StatusCode = StatusCodeResponse.Success,
                Content = new BusinessLicenseProviderUpload("", "image", "upload", "https://example.com/provider.pdf") });
        var response = await Service.UploadBusinessLicenseAsync(File(content), 7);
        Assert.Equal(StatusCodeResponse.Error, response.StatusCode);
        Assert.Null(response.Content);
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DatabaseException_DoesNotExposeReceipt(bool cancelled)
    {
        await using var content = new MemoryStream("%PDF-1.7"u8.ToArray());
        SetupProvider();
        _uploads.Setup(repo => repo.AddAsync(It.IsAny<BusinessLicenseUpload>(), default)).Returns(Task.CompletedTask);
        var exception = cancelled ? new OperationCanceledException() : new Exception("Database unavailable");
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ThrowsAsync(exception);
        if (cancelled) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service.UploadBusinessLicenseAsync(File(content), 7));
        else
        {
            var response = await Service.UploadBusinessLicenseAsync(File(content), 7);
            Assert.Equal(StatusCodeResponse.Error, response.StatusCode);
            Assert.Null(response.Content);
        }
    }

    [Fact]
    public async Task OverlongFilename_IsRejectedBeforeProviderOrDatabase()
    {
        await using var content = new MemoryStream("%PDF-1.7"u8.ToArray());
        var file = File(content);
        file.FileName = new string('a', 252) + ".pdf";
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadBusinessLicenseAsync(file, 7)).StatusCode);
        _provider.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    private void SetupProvider() => _provider.Setup(provider => provider.UploadAsync(It.IsAny<UploadFileDTO>(), 7, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ApiResponse<BusinessLicenseProviderUpload> { StatusCode = StatusCodeResponse.Success,
            Content = new BusinessLicenseProviderUpload("asset/server-id", "image", "upload", "https://example.com/provider.pdf") });
    private static UploadFileDTO File(Stream content) =>
        new() { FileName = "license.pdf", ContentType = "APPLICATION/PDF", Size = 8, Content = content };

    private static void AssertMetadata(BusinessLicenseUpload row, DateTime before)
    {
        Assert.Equal(7, row.OwnerId);
        Assert.Equal("asset/server-id", row.PublicId);
        Assert.Equal(("image", "upload"), (row.ResourceType, row.DeliveryType));
        Assert.Equal("https://example.com/provider.pdf", row.StoredUrl);
        Assert.InRange(row.CreatedAt, before, DateTime.UtcNow);
        Assert.Equal(("license.pdf", "application/pdf", 8L, false), (row.OriginalFileName, row.ContentType, row.SizeBytes, row.IsDeleted));
    }
}
