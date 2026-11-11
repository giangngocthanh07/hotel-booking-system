using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using HotelBooking.application.Validators.Common;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.HotelManagement.Owner;

[Trait("Requirement", "FR-CAT-001")]
[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-CAT-008")]
public class OwnerHotelServiceTests
{
    private readonly Mock<IHotelRepository> _hotels = new(MockBehavior.Strict);
    private OwnerHotelService Service => new(_hotels.Object, new PagingRequestValidator());

    [Fact]
    public async Task Details_ReturnsInactiveHotelWithoutStorageMetadata()
    {
        using var cancellation = new CancellationTokenSource();
        _hotels.Setup(repo => repo.GetByOwnerAsync(7, 42, cancellation.Token)).ReturnsAsync(new Hotel
        {
            Id = 42, OwnerId = 7, Name = "Preparing hotel", Address = "Address", Status = "Inactive", IsVerified = true,
            Additional = "{\"PublicPhone\":\"0901234567\",\"Latitude\":0,\"Longitude\":106,\"BusinessLicenseUrl\":\"secret\"}",
        });
        var result = await Service.GetByIdAsync(7, 42, cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("Inactive", result.Content!.Status);
        Assert.True(result.Content.IsVerified);
        Assert.Equal(0, result.Content.Latitude);
        Assert.Equal("0901234567", result.Content.PublicPhone);
        Assert.DoesNotContain("secret", System.Text.Json.JsonSerializer.Serialize(result.Content));
        _hotels.VerifyAll();
    }

    [Fact]
    public async Task MissingOrForeignHotel_ReturnsNotFound()
    {
        _hotels.Setup(repo => repo.GetByOwnerAsync(7, 42, default)).ReturnsAsync((Hotel?)null);
        Assert.Equal(StatusCodeResponse.NotFound, (await Service.GetByIdAsync(7, 42)).StatusCode);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 101)]
    [InlineData(2147483647, 100)]
    public async Task InvalidPaging_DoesNotReadDatabase(int page, int size)
    {
        var result = await Service.GetPageAsync(7, new PagingRequest { PageIndex = page, PageSize = size });
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        _hotels.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Page_ForwardsScopePaginationAndCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        _hotels.Setup(repo => repo.GetPageByOwnerAsync(7, 2, 10, cancellation.Token))
            .ReturnsAsync((new List<Hotel> { new() { Id = 42, Name = "Hotel", Status = "Inactive" } }, 11));
        var result = await Service.GetPageAsync(7, new PagingRequest { PageIndex = 2, PageSize = 10 }, cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(11, result.Content!.TotalCount);
        Assert.Equal(2, result.Content.TotalPages);
        Assert.Equal("Inactive", Assert.Single(result.Content.Items).Status);
        _hotels.VerifyAll();
    }

    [Fact]
    public async Task PreCancelledQuery_DoesNotReadDatabase()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service.GetByIdAsync(7, 42, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service.GetPageAsync(7, new(), cancellation.Token));
        _hotels.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidOwner_DoesNotReadDatabase(int ownerId)
    {
        Assert.Equal(StatusCodeResponse.Unauthorized, (await Service.GetPageAsync(ownerId, new())).StatusCode);
        Assert.Equal(StatusCodeResponse.Unauthorized, (await Service.GetByIdAsync(ownerId, 42)).StatusCode);
        _hotels.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidHotelId_DoesNotReadDatabase(int hotelId)
    {
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.GetByIdAsync(7, hotelId)).StatusCode);
        _hotels.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not json")]
    public async Task MissingOrLegacyMetadata_PreservesBasicHotelDetails(string? metadata)
    {
        _hotels.Setup(repo => repo.GetByOwnerAsync(7, 42, default))
            .ReturnsAsync(new Hotel { Id = 42, Name = "Legacy Hotel", Additional = metadata });
        var result = await Service.GetByIdAsync(7, 42);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("Legacy Hotel", result.Content!.Name);
        Assert.Equal(string.Empty, result.Content.PublicPhone);
    }
}
