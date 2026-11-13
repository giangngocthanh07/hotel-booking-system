using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HotelBooking.test.UnitTests.Services.HotelManagement.Owner.Images;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-CAT-006")]
[Trait("Requirement", "FR-MED-005")]
public class OwnerHotelImageDeleteServiceTests
{
    private readonly Mock<IHotelImageRepository> _images = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unit = new(MockBehavior.Strict);
    private OwnerHotelImageDeleteService Service => new(_images.Object, _unit.Object, NullLogger<OwnerHotelImageDeleteService>.Instance);

    [Fact]
    public async Task ForeignHotel_RollsBackWithoutLookingUpImage()
    {
        _unit.Setup(unit => unit.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _images.Setup(images => images.GetOwnedForMediaWriteAsync(1, 2, default)).ReturnsAsync((Hotel?)null);
        _unit.Setup(unit => unit.RollBackTransactionAsync(CancellationToken.None)).Returns(Task.CompletedTask);
        Assert.Equal(StatusCodeResponse.NotFound, (await Service.DeleteAsync(1, 2, 3)).StatusCode);
        _images.Verify(images => images.GetGalleryImageForWriteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActiveHotel_RejectsRemovalBelowFourDistinctImages()
    {
        SetupOwned("Active");
        _images.Setup(images => images.GetAdditionalImageCountAsync(2, "cover", default)).ReturnsAsync(4);
        _images.Setup(images => images.GetAdditionalImageCountExcludingAsync(2, "cover", 3, default)).ReturnsAsync(3);
        _unit.Setup(unit => unit.RollBackTransactionAsync(CancellationToken.None)).Returns(Task.CompletedTask);
        var result = await Service.DeleteAsync(1, 2, 3);
        Assert.Equal(StatusCodeResponse.Conflict, result.StatusCode);
        Assert.Contains("replacement", result.Message);
        _unit.Verify(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InactiveHotel_SoftDeletesOnlySelectedImageAndCommits()
    {
        var image = SetupOwned("Inactive");
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(1, false));
        _unit.Setup(unit => unit.CommitTransactionAsync(default)).Returns(Task.CompletedTask);
        Assert.Equal(StatusCodeResponse.Success, (await Service.DeleteAsync(1, 2, 3)).StatusCode);
        Assert.True(image.IsDeleted);
        _unit.Verify(unit => unit.CommitTransactionAsync(default), Times.Once);
    }

    private HotelImage SetupOwned(string status)
    {
        _unit.Setup(unit => unit.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _images.Setup(images => images.GetOwnedForMediaWriteAsync(1, 2, default)).ReturnsAsync(new Hotel { Id = 2, OwnerId = 1, Status = status, CoverImageUrl = "cover" });
        var image = new HotelImage { Id = 3, HotelId = 2, ImageUrl = "gallery", IsDeleted = false };
        _images.Setup(images => images.GetGalleryImageForWriteAsync(2, 3, default)).ReturnsAsync(image);
        return image;
    }

    [Fact]
    public async Task SaveFailure_RollsBackWithoutCommit()
    {
        SetupOwned("Inactive");
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(0, true));
        _unit.Setup(unit => unit.RollBackTransactionAsync(CancellationToken.None)).Returns(Task.CompletedTask);
        Assert.Equal(StatusCodeResponse.Conflict, (await Service.DeleteAsync(1, 2, 3)).StatusCode);
        _unit.Verify(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancellationDuringSave_RollsBackAndPropagates()
    {
        SetupOwned("Inactive");
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ThrowsAsync(new OperationCanceledException());
        _unit.Setup(unit => unit.RollBackTransactionAsync(CancellationToken.None)).Returns(Task.CompletedTask);
        await Assert.ThrowsAsync<OperationCanceledException>(() => Service.DeleteAsync(1, 2, 3));
        _unit.Verify(unit => unit.RollBackTransactionAsync(CancellationToken.None), Times.Once);
    }
}
