using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.HotelManagement.Owner.Images;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-MED-005")]
public class OwnerHotelImageReadServiceTests
{
    [Fact]
    public async Task OwnedHotel_ReturnsCoverAndOrderedImageMetadata()
    {
        using var cancellation = new CancellationTokenSource();
        var repository = new Mock<IHotelImageRepository>(MockBehavior.Strict);
        repository.Setup(repo => repo.GetOwnedGalleryAsync(7, 42, cancellation.Token)).ReturnsAsync(new Hotel
        {
            Id = 42, CoverImageUrl = "https://example.com/cover.jpg", Status = "Inactive",
            HotelImages = [new HotelImage { Id = 9, ImageUrl = "https://example.com/a.jpg", DisplayOrder = 0 }],
        });
        var result = await new OwnerHotelImageReadService(repository.Object).GetAsync(7, 42, cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(42, result.Content!.HotelId);
        Assert.Equal("https://example.com/cover.jpg", result.Content.CoverImageUrl);
        Assert.Equal(9, Assert.Single(result.Content.Gallery).Id);
        repository.VerifyAll();
    }

    [Fact]
    public async Task MissingOrForeignHotel_ReturnsNotFound()
    {
        var repository = new Mock<IHotelImageRepository>();
        repository.Setup(repo => repo.GetOwnedGalleryAsync(7, 42, default)).ReturnsAsync((Hotel?)null);
        Assert.Equal(StatusCodeResponse.NotFound, (await new OwnerHotelImageReadService(repository.Object).GetAsync(7, 42)).StatusCode);
    }

    [Theory]
    [InlineData(0, 42, StatusCodeResponse.Unauthorized)]
    [InlineData(7, 0, StatusCodeResponse.BadRequest)]
    public async Task InvalidIdentityOrHotelId_DoesNotQuery(int ownerId, int hotelId, string status)
    {
        var repository = new Mock<IHotelImageRepository>(MockBehavior.Strict);
        Assert.Equal(status, (await new OwnerHotelImageReadService(repository.Object).GetAsync(ownerId, hotelId)).StatusCode);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PreCancelledRead_DoesNotQuery()
    {
        var repository = new Mock<IHotelImageRepository>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new OwnerHotelImageReadService(repository.Object).GetAsync(7, 42, cancellation.Token));
        repository.VerifyNoOtherCalls();
    }
}
