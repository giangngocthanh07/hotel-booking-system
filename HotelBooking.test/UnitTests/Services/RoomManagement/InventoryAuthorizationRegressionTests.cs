using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.application.Validators.RoomManagement;
using Moq;
using AppRoomService = HotelBooking.application.Services.Domains.RoomManagement.RoomService;

namespace HotelBooking.test.UnitTests.Services.RoomManagement;

[Trait("Requirement", "FR-IAM-011")]
[Trait("Requirement", "FR-INV-013")]
// Prevents the original authorization bypass from returning through future refactoring.
public class InventoryAuthorizationRegressionTests : BaseServiceTest<AppRoomService>
{
    [Fact]
    public async Task RoomList_WithoutOwnedRoomType_DoesNotReadRooms()
    {
        // Arrange: any room query is unexpected when the caller has no access.
        var rooms = new Mock<IRoomRepository>(MockBehavior.Strict);
        // An unconfigured boolean access check returns false, simulating denied ownership.
        var access = new Mock<IInventoryAccessService>();
        var service = new AppRoomService(rooms.Object, access.Object, new BatchAddRoomsValidator(), new RoomListQueryValidator(),
            _mockUnitOfWork.Object, _mockLogger.Object);
        // Act: owner 7 requests room type 99 without an ownership grant.
        var result = await service.GetRoomsByRoomTypeAsync(99, 7);
        // Assert both the denial and the absence of reads that could expose private data.
        Assert.Equal(StatusCodeResponse.Forbidden, result.StatusCode);
        Assert.Empty(rooms.Invocations);
    }
}
