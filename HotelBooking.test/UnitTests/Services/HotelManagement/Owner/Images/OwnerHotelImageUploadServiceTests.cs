using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Validators.HotelManagement.Owner.Images;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HotelBooking.test.UnitTests.Services.HotelManagement.Owner.Images;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-MED-001")]
[Trait("Requirement", "FR-MED-002")]
[Trait("Requirement", "FR-MED-003")]
public class OwnerHotelImageUploadServiceTests
{
    private readonly Mock<IHotelImageRepository> _images = new(MockBehavior.Strict);
    private readonly Mock<IPhotoService> _photos = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unit = new(MockBehavior.Strict);
    private OwnerHotelImageUploadService Service => new(_images.Object, _photos.Object, _unit.Object,
        new OwnerHotelImageUploadValidator(), NullLogger<OwnerHotelImageUploadService>.Instance);

    [Fact]
    public async Task ForeignHotel_DoesNotReadFileUploadOrSave()
    {
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        _images.Setup(repo => repo.OwnsHotelAsync(7, 42, default)).ReturnsAsync(false);
        var result = await Service.UploadAsync(7, 42, File(content), false);
        Assert.Equal(StatusCodeResponse.NotFound, result.StatusCode);
        Assert.Equal(0, content.Position);
        _photos.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidSignature_DoesNotReachProvider()
    {
        await using var content = new MemoryStream(new byte[] { 1, 2, 3 });
        _images.Setup(repo => repo.OwnsHotelAsync(7, 42, default)).ReturnsAsync(true);
        var result = await Service.UploadAsync(7, 42, File(content), false);
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        _photos.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cover_ReplacesOnlyCoverAfterProviderSuccessAndOneSave()
    {
        var hotel = new Hotel { Id = 42, OwnerId = 7, Status = "Inactive", CoverImageUrl = "old.jpg" };
        Setup(hotel);
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        var result = await Service.UploadAsync(7, 42, File(content), true);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("https://example.com/new.jpg", hotel.CoverImageUrl);
        Assert.Equal("Inactive", hotel.Status);
        Assert.Null(result.Content!.ImageId);
        _unit.Verify(unit => unit.TrySaveChangesAsync(default), Times.Once);
        _unit.Verify(unit => unit.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task Gallery_AppendsAfterHighestPositionIncludingDeletedRows()
    {
        Setup(new Hotel { Id = 42, OwnerId = 7, Status = "Inactive" });
        _images.Setup(repo => repo.GetMaximumDisplayOrderAsync(42, default)).ReturnsAsync(7);
        _images.Setup(repo => repo.AddGalleryImageAsync(It.IsAny<HotelImage>(), default)).Returns(Task.CompletedTask);
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        var result = await Service.UploadAsync(7, 42, File(content), false);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(8, result.Content!.DisplayOrder);
        _images.Verify(repo => repo.AddGalleryImageAsync(It.Is<HotelImage>(image => image.HotelId == 42 && image.DisplayOrder == 8 && image.IsDeleted == false), default), Times.Once);
    }

    [Fact]
    public async Task ProviderFailure_KeepsOldCoverAndDoesNotOpenTransaction()
    {
        _images.Setup(repo => repo.OwnsHotelAsync(7, 42, default)).ReturnsAsync(true);
        _photos.Setup(photo => photo.UploadHotelImageAsync(It.IsAny<UploadFileDTO>(), 7, 42, true, default))
            .ReturnsAsync(ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, "Image rejected."));
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadAsync(7, 42, File(content), true)).StatusCode);
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OwnershipChangedAfterProvider_DoesNotSave()
    {
        Setup(new Hotel { Id = 42 });
        _images.Setup(repo => repo.GetOwnedForMediaWriteAsync(7, 42, default)).ReturnsAsync((Hotel?)null);
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        Assert.Equal(StatusCodeResponse.NotFound, (await Service.UploadAsync(7, 42, File(content), true)).StatusCode);
        _unit.Verify(unit => unit.RollBackTransactionAsync(CancellationToken.None), Times.Once);
        _unit.Verify(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(false, StatusCodeResponse.Error)]
    [InlineData(true, StatusCodeResponse.Conflict)]
    public async Task SaveFailure_DoesNotReturnReceiptOrCommit(bool conflict, string status)
    {
        Setup(new Hotel { Id = 42, Status = "Inactive" });
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(0, conflict));
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        var result = await Service.UploadAsync(7, 42, File(content), true);
        Assert.Equal(status, result.StatusCode);
        Assert.Null(result.Content);
        _unit.Verify(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unit.Verify(unit => unit.RollBackTransactionAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ActiveCover_CannotReduceFourAdditionalImagesToThree()
    {
        var hotel = new Hotel { Id = 42, Status = "Active", CoverImageUrl = "old.jpg" };
        Setup(hotel);
        _images.Setup(repo => repo.GetAdditionalImageCountAsync(42, "old.jpg", default)).ReturnsAsync(4);
        _images.Setup(repo => repo.GetAdditionalImageCountAsync(42, "https://example.com/new.jpg", default)).ReturnsAsync(3);
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        Assert.Equal(StatusCodeResponse.Conflict, (await Service.UploadAsync(7, 42, File(content), true)).StatusCode);
        Assert.Equal("old.jpg", hotel.CoverImageUrl);
        _unit.Verify(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveCancellation_RollsBackAndPropagates()
    {
        Setup(new Hotel { Id = 42, Status = "Inactive" });
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ThrowsAsync(new OperationCanceledException());
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service.UploadAsync(7, 42, File(content), true));
        _unit.Verify(unit => unit.RollBackTransactionAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ActualOversizedContent_IsBoundedBeforeProvider()
    {
        _images.Setup(repo => repo.OwnsHotelAsync(7, 42, default)).ReturnsAsync(true);
        await using var content = new MemoryStream(new byte[10 * 1024 * 1024 + 100]);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadAsync(7, 42, File(content), false)).StatusCode);
        Assert.Equal(10 * 1024 * 1024 + 1, content.Position);
        _photos.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    private void Setup(Hotel hotel)
    {
        _images.Setup(repo => repo.OwnsHotelAsync(7, 42, default)).ReturnsAsync(true);
        _photos.Setup(photo => photo.UploadHotelImageAsync(It.IsAny<UploadFileDTO>(), 7, 42, It.IsAny<bool>(), default))
            .ReturnsAsync(ResponseFactory.Success(new UploadResultDTO { Uploaded = true, StoredFileName = "https://example.com/new.jpg" }, "Uploaded."));
        _unit.Setup(unit => unit.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _images.Setup(repo => repo.GetOwnedForMediaWriteAsync(7, 42, default)).ReturnsAsync(hotel);
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(1, false));
        _unit.Setup(unit => unit.CommitTransactionAsync(default)).Returns(Task.CompletedTask);
        _unit.Setup(unit => unit.RollBackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    private static OwnerHotelImageFileDTO File(Stream content) => new() { FileName = "hotel.jpg", ContentType = "image/jpeg", Size = 3, Content = content };
}
