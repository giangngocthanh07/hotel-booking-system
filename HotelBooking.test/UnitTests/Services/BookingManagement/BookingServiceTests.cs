using Moq;
using HotelBooking.application.Services.Domains.BookingManagement;
using HotelBooking.application.DTOs.Booking;
using HotelBooking.infrastructure.Models;
using System.Linq.Expressions;

using AppBookingService = HotelBooking.application.Services.Domains.BookingManagement.BookingService;

namespace HotelBooking.test.UnitTests.Services.BookingManagement;

public class BookingServiceTests : BaseServiceTest<AppBookingService>
{
    private readonly Mock<IRoomTypeRepository> _mockRoomTypeRepo;
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IBookingRoomRepository> _mockBookingRoomRepo;
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly AppBookingService _service;

    public BookingServiceTests()
    {
        _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockBookingRoomRepo = new Mock<IBookingRoomRepository>();
        _mockHotelRepo = new Mock<IHotelRepository>();

        _service = new AppBookingService(
            _mockRoomTypeRepo.Object,
            _mockRoomRepo.Object,
            _mockBookingRepo.Object,
            _mockBookingRoomRepo.Object,
            _mockHotelRepo.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateBookingAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        int userId = 1;
        var request = new BookingRequestDTO
        {
            HotelId = 1,
            RoomTypeId = 1,
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfRooms = 1
        };

        var roomType = new RoomType { Id = 1, PricePerNight = 1000000 };
        var availableRooms = new List<Room> { new Room { Id = 1, RoomTypeId = 1 } };

        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(request.RoomTypeId)).ReturnsAsync(roomType);
        _mockRoomRepo.Setup(r => r.GetAvailableRoomsAsync(request.RoomTypeId, request.CheckInDate, request.CheckOutDate, request.NumberOfRooms))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _service.CreateBookingAsync(request, userId);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(result.Content);
        Assert.Equal(2000000, result.Content.TotalPrice); // 1.000.000 x 2 nights
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_NoAvailableRooms_ReturnsError()
    {
        // Arrange
        int userId = 1;
        var request = new BookingRequestDTO
        {
            HotelId = 1,
            RoomTypeId = 1,
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfRooms = 1
        };

        var roomType = new RoomType { Id = 1, PricePerNight = 1000000 };
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(request.RoomTypeId)).ReturnsAsync(roomType);
        _mockRoomRepo.Setup(r => r.GetAvailableRoomsAsync(request.RoomTypeId, request.CheckInDate, request.CheckOutDate, request.NumberOfRooms))
            .ReturnsAsync(new List<Room>()); // No rooms available

        // Act
        var result = await _service.CreateBookingAsync(request, userId);

        // Assert
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Contains("Not enough rooms available", result.Message);
        _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGuestBookingsAsync_ReturnsBookings()
    {
        // Arrange
        int userId = 1;
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, CustomerId = userId, HotelId = 1, CheckInDate = DateOnly.FromDateTime(DateTime.Today), CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) }
        };

        _mockBookingRepo.Setup(r => r.WhereAsync(It.IsAny<Expression<Func<Booking, bool>>>())).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetGuestBookingsAsync(userId, "All");

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Single(result.Content);
    }

    [Fact]
    public async Task GetOwnerBookingsAsync_ReturnsHotelBookings()
    {
        // Arrange
        int ownerId = 1;
        var hotels = new List<Hotel> { new Hotel { Id = 1, OwnerId = ownerId } };
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, HotelId = 1, CustomerId = 2, CheckInDate = DateOnly.FromDateTime(DateTime.Today), CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) }
        };

        _mockHotelRepo.Setup(r => r.WhereAsync(It.IsAny<Expression<Func<Hotel, bool>>>())).ReturnsAsync(hotels);
        _mockBookingRepo.Setup(r => r.GetBookingsByHotelsAsync(It.IsAny<List<int>>())).ReturnsAsync(bookings);

        // Act
        var result = await _service.GetOwnerBookingsAsync(ownerId, "All", null);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
    }
}
