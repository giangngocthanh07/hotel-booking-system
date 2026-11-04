using System.Linq.Expressions;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.application.Validators.RoomManagement;
using HotelBooking.infrastructure.Models;
using Moq;
using AppRoomService = HotelBooking.application.Services.Domains.RoomManagement.RoomService;

namespace HotelBooking.test.UnitTests.Services.RoomManagement;

[Trait("Requirement", "FR-IAM-011")]
[Trait("Requirement", "FR-INV-013")]
public class RoomServiceTests : BaseServiceTest<AppRoomService>
{
    private readonly Mock<IRoomRepository> _rooms = new();
    private readonly Mock<IInventoryAccessService> _access = new();
    private readonly AppRoomService _service;

    public RoomServiceTests()
    {
        _access.Setup(access => access.CanAccessRoomTypeAsync(2, 7, It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _service = new(_rooms.Object, _access.Object, new BatchAddRoomsValidator(), new RoomListQueryValidator(),
            _mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Batch_ValidOwner_CreatesRoomsAndSavesOnce()
    {
        var result = await _service.BatchAddRoomsAsync(ValidRequest(), 7);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(result.Content);
        Assert.Equal(2, result.Content.Count());
        _rooms.Verify(rooms => rooms.AddAsync(It.Is<Room>(room => room.RoomTypeId == 2 && room.IsDeleted == false)), Times.Exactly(2));
        Verify_Saved();
        _mockUnitOfWork.Verify(unit => unit.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task Batch_Denied_DoesNotReadRoomsOrOpenTransaction()
    {
        var result = await _service.BatchAddRoomsAsync(ValidRequest(), 8);
        Assert.Equal(StatusCodeResponse.Forbidden, result.StatusCode);
        Assert.Equal(InventoryAccessService.AccessDeniedMessage, result.Message);
        Assert.Empty(_rooms.Invocations);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Fact]
    public async Task List_Denied_DoesNotReadRooms()
    {
        var result = await _service.GetRoomsByRoomTypeAsync(2, 8);
        Assert.Equal(StatusCodeResponse.Forbidden, result.StatusCode);
        Assert.Equal(InventoryAccessService.AccessDeniedMessage, result.Message);
        Assert.Empty(_rooms.Invocations);
    }

    [Fact]
    public async Task List_ValidOwner_FiltersRoomTypeAndDeletedRooms()
    {
        var data = new[] { new Room { RoomTypeId = 2, RoomNumber = "101" }, new Room { RoomTypeId = 3 }, new Room { RoomTypeId = 2, IsDeleted = true } };
        _rooms.Setup(rooms => rooms.WhereAsync(It.IsAny<Expression<Func<Room, bool>>>()))
            .ReturnsAsync((Expression<Func<Room, bool>> predicate) => data.Where(predicate.Compile()));
        var result = await _service.GetRoomsByRoomTypeAsync(2, 7);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(result.Content);
        Assert.Equal("101", Assert.Single(result.Content).RoomNumber);
        Assert.Equal("Active", Assert.Single(result.Content).Status);
    }

    [Fact]
    public async Task Batch_DuplicateExistingRoom_ReturnsConflictBeforeTransaction()
    {
        _rooms.Setup(rooms => rooms.AnyAsync(It.IsAny<Expression<Func<Room, bool>>>())).ReturnsAsync(true);
        var result = await _service.BatchAddRoomsAsync(ValidRequest(), 7);
        Assert.Equal(StatusCodeResponse.Conflict, result.StatusCode);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task List_InvalidId_ValidationPrecedesAccess(int roomTypeId)
    {
        var result = await _service.GetRoomsByRoomTypeAsync(roomTypeId, 7);
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        Assert.Empty(_access.Invocations);
        Assert.Empty(_rooms.Invocations);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("hotel")]
    [InlineData("type")]
    [InlineData("empty")]
    [InlineData("blank")]
    [InlineData("nullNumbers")]
    public async Task Batch_InvalidRequest_ValidationPrecedesAccess(string invalid)
    {
        var request = ValidRequest();
        if (invalid == "hotel") request.HotelId = 0;
        if (invalid == "type") request.RoomTypeId = -1;
        if (invalid == "empty") request.RoomNumbers = [];
        if (invalid == "blank") request.RoomNumbers = [" "];
        if (invalid == "nullNumbers") request.RoomNumbers = null!;
        var result = await _service.BatchAddRoomsAsync(invalid == "null" ? null! : request, 7);
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        Assert.Empty(_access.Invocations);
        Assert.Empty(_rooms.Invocations);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancelledRequest_ThrowsWithoutRoomReadsOrWrites(bool batch)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => batch
            ? _service.BatchAddRoomsAsync(ValidRequest(), 7, cancellation.Token)
            : _service.GetRoomsByRoomTypeAsync(2, 7, cancellation.Token));
        Assert.Empty(_rooms.Invocations);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Fact]
    public async Task Batch_CancelledDuringAccess_ThrowsBeforeTransaction()
    {
        using var cancellation = new CancellationTokenSource();
        _access.Setup(access => access.CanAccessRoomTypeAsync(2, 7, 1, cancellation.Token))
            .Callback(() => cancellation.Cancel()).ReturnsAsync(true);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.BatchAddRoomsAsync(ValidRequest(), 7, cancellation.Token));
        Assert.Empty(_rooms.Invocations);
        Assert.Empty(_mockUnitOfWork.Invocations);
    }

    [Fact]
    public async Task Batch_CancelledDuringAdd_RollsBackWithoutSave()
    {
        using var cancellation = new CancellationTokenSource();
        _rooms.Setup(rooms => rooms.AddAsync(It.IsAny<Room>())).Callback(() => cancellation.Cancel()).Returns(Task.CompletedTask);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _service.BatchAddRoomsAsync(ValidRequest(), 7, cancellation.Token));
        Verify_Never_Saved();
        _mockUnitOfWork.Verify(unit => unit.RollBackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(unit => unit.CommitTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task Batch_SaveFailure_RollsBackWithoutCommit()
    {
        _mockUnitOfWork.Setup(unit => unit.SaveChangesAsync()).ThrowsAsync(new Exception("database"));
        var result = await _service.BatchAddRoomsAsync(ValidRequest(), 7);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        _mockUnitOfWork.Verify(unit => unit.RollBackTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(unit => unit.CommitTransactionAsync(), Times.Never);
    }

    private static BatchAddRoomsRequestDTO ValidRequest() => new() { HotelId = 1, RoomTypeId = 2, RoomNumbers = ["101", "102"] };
}
