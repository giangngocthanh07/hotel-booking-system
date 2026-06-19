using Moq;
using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.infrastructure.Models;
using HotelBooking.application.Interfaces;
using System.Linq.Expressions;

using AppRoomService = HotelBooking.application.Services.Domains.RoomManagement.RoomService;

namespace HotelBooking.test.UnitTests.Services.RoomManagement;

public class RoomServiceTests : BaseServiceTest<AppRoomService>
{
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IRoomTypeRepository> _mockRoomTypeRepo;
    private readonly AppRoomService _service;

    public RoomServiceTests()
    {
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();

        _service = new AppRoomService(
            _mockRoomRepo.Object,
            _mockRoomTypeRepo.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task BatchAddRoomsAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new BatchAddRoomsRequestDTO
        {
            HotelId = 1,
            RoomTypeId = 1,
            RoomNumbers = new List<string> { "101", "102" },
            Status = "Active"
        };

        var roomType = new RoomType { Id = 1, HotelId = 1 };
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(request.RoomTypeId)).ReturnsAsync(roomType);
        _mockRoomRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Room, bool>>>())).ReturnsAsync(false);

        // Act
        var result = await _service.BatchAddRoomsAsync(request);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(2, result.Content.Count());
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task BatchAddRoomsAsync_DuplicateRoomNumber_ReturnsError()
    {
        // Arrange
        var request = new BatchAddRoomsRequestDTO
        {
            HotelId = 1,
            RoomTypeId = 1,
            RoomNumbers = new List<string> { "101" }
        };

        var roomType = new RoomType { Id = 1, HotelId = 1 };
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(request.RoomTypeId)).ReturnsAsync(roomType);
        _mockRoomRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Room, bool>>>())).ReturnsAsync(true); // Already exists

        // Act
        var result = await _service.BatchAddRoomsAsync(request);

        // Assert
        Assert.Equal(StatusCodeResponse.Conflict, result.StatusCode);
        _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Once);
    }
}
