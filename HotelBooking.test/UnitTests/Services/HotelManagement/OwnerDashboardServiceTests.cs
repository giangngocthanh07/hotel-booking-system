using Moq;
using HotelBooking.application.Services.Domains.HotelManagement;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.infrastructure.Models;
using System.Linq.Expressions;

namespace HotelBooking.test.UnitTests.Services.HotelManagement;

public class OwnerDashboardServiceTests : BaseServiceTest<OwnerDashboardService>
{
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IPaymentRepository> _mockPaymentRepo;
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IRoomTypeRepository> _mockRoomTypeRepo;
    private readonly OwnerDashboardService _service;

    public OwnerDashboardServiceTests()
    {
        _mockHotelRepo = new Mock<IHotelRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockPaymentRepo = new Mock<IPaymentRepository>();
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();

        _service = new OwnerDashboardService(
            _mockHotelRepo.Object,
            _mockBookingRepo.Object,
            _mockPaymentRepo.Object,
            _mockRoomRepo.Object,
            _mockRoomTypeRepo.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetOwnerDashboardStatsAsync_ReturnsCorrectAggregation()
    {
        // Arrange
        int ownerId = 1;
        var ownedHotels = new List<Hotel> { new Hotel { Id = 1, OwnerId = ownerId } };
        var bookings = new List<Booking> 
        { 
            new Booking { Id = 1, HotelId = 1, CheckInDate = DateOnly.FromDateTime(DateTime.Today), CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)), Status = "Success" } 
        };
        var payments = new List<Payment> 
        { 
            new Payment { Amount = 1000, Status = "Success", PaidAt = DateTime.Now }
        };
        var rooms = new List<Room> { new Room { Id = 1, RoomTypeId = 1 } };
        var roomTypes = new List<RoomType> { new RoomType { Id = 1, Name = "Deluxe", HotelId = 1 } };

        _mockHotelRepo.Setup(r => r.WhereAsync(It.IsAny<Expression<Func<Hotel, bool>>>())).ReturnsAsync(ownedHotels);
        _mockBookingRepo.Setup(r => r.GetBookingsByHotelsAsync(It.IsAny<List<int>>())).ReturnsAsync(bookings);
        _mockPaymentRepo.Setup(r => r.GetPaymentsByHotelsAsync(It.IsAny<List<int>>())).ReturnsAsync(payments);
        _mockRoomRepo.Setup(r => r.GetRoomsByHotelsAsync(It.IsAny<List<int>>())).ReturnsAsync(rooms);
        _mockRoomTypeRepo.Setup(r => r.WhereAsync(It.IsAny<Expression<Func<RoomType, bool>>>())).ReturnsAsync(roomTypes);

        // Act
        var result = await _service.GetOwnerDashboardStatsAsync(ownerId);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(1, result.Content.TodayArrivals);
        Assert.Equal(1, result.Content.TotalStaying);
        Assert.Equal(1000, result.Content.TotalRevenue);
        Assert.Equal(100, result.Content.OccupancyRate);
    }
}
