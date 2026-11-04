using System.Linq.Expressions;
using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RoomManagement;

[Trait("Requirement", "FR-IAM-011")]
[Trait("Requirement", "FR-INV-013")]
// Exercises the real authorization predicates against in-memory repository data.
// These tests do not verify EF query translation or database execution.
public class InventoryAccessServiceTests
{
    private readonly Mock<IHotelRepository> _hotels = new();
    private readonly Mock<IRoomTypeRepository> _types = new();
    private readonly InventoryAccessService _service;

    public InventoryAccessServiceTests() => _service = new(_hotels.Object, _types.Object);

    [Theory]
    [InlineData(1, 7, false, true)]
    [InlineData(1, 7, null, true)] // Nullable IsDeleted follows the existing "not true" convention.
    [InlineData(1, 8, false, false)]
    [InlineData(2, 7, false, false)]
    [InlineData(1, 7, true, false)]
    public async Task HotelAccess_RequiresRequestedOwnerAndLiveHotel(int id, int owner, bool? deleted, bool expected)
    {
        var data = new[] { new Hotel { Id = id, OwnerId = owner, IsDeleted = deleted } };
        // Evaluate the service's predicate instead of returning a hardcoded permission result.
        _hotels.Setup(hotels => hotels.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync((Expression<Func<Hotel, bool>> predicate) => data.Any(predicate.Compile()));
        Assert.Equal(expected, await _service.CanAccessHotelAsync(1, 7));
    }

    [Theory]
    [InlineData(2, 1, 7, false, false, null, true)] // Listing does not supply a HotelId.
    [InlineData(2, 1, 7, null, null, 1, true)]
    [InlineData(3, 1, 7, false, false, 1, false)]
    [InlineData(2, 1, 8, false, false, 1, false)]
    [InlineData(2, 1, 7, true, false, 1, false)]
    [InlineData(2, 1, 7, false, true, 1, false)]
    [InlineData(2, 1, 7, false, false, 9, false)] // Batch creation must match the requested HotelId.
    public async Task RoomTypeAccess_RequiresLiveTypeAndLiveOwnedHotel(
        int id, int hotelId, int owner, bool? typeDeleted, bool? hotelDeleted, int? requestedHotel, bool expected)
    {
        var data = new[] { new RoomType { Id = id, HotelId = hotelId, IsDeleted = typeDeleted,
            Hotel = new Hotel { Id = hotelId, OwnerId = owner, IsDeleted = hotelDeleted } } };
        _types.Setup(types => types.AnyAsync(It.IsAny<Expression<Func<RoomType, bool>>>()))
            .ReturnsAsync((Expression<Func<RoomType, bool>> predicate) => data.Any(predicate.Compile()));
        Assert.Equal(expected, await _service.CanAccessRoomTypeAsync(2, 7, requestedHotel));
    }

    [Fact]
    public async Task MissingInventory_DeniesAccess()
    {
        // No matching repository result must grant access by default.
        Assert.False(await _service.CanAccessHotelAsync(1, 7));
        Assert.False(await _service.CanAccessRoomTypeAsync(2, 7));
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(1, 0)]
    [InlineData(-1, 7)]
    [InlineData(1, -1)]
    public async Task InvalidIds_DenyWithoutQueries(int resource, int owner)
    {
        // Invalid identity/resource IDs should fail before any database query.
        Assert.False(await _service.CanAccessHotelAsync(resource, owner));
        Assert.False(await _service.CanAccessRoomTypeAsync(resource, owner));
        Assert.Empty(_hotels.Invocations);
        Assert.Empty(_types.Invocations);
    }

    [Fact]
    public async Task CancelledAccess_ThrowsWithoutQueries()
    {
        // Cancellation must propagate rather than being converted into an access decision.
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.CanAccessHotelAsync(1, 7, cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.CanAccessRoomTypeAsync(2, 7, cancellationToken: cancellation.Token));
        Assert.Empty(_hotels.Invocations);
        Assert.Empty(_types.Invocations);
    }
}
