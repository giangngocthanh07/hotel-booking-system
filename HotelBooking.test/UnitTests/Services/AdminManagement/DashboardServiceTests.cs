using Moq;
using HotelBooking.application.Services.Domains.AdminManagement;
using HotelBooking.infrastructure.Models;
using System.Globalization;

namespace HotelBooking.test.UnitTests.Services.AdminManagement;

public class DashboardServiceTests : BaseServiceTest<DashboardService>
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IPaymentRepository> _mockPaymentRepo;
    private readonly Mock<IHotelApprovalRequestRepository> _mockHotelApprovalRepo;
    private readonly Mock<IUpgradeRequestRepository> _mockUpgradeRepo;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockHotelRepo = new Mock<IHotelRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockPaymentRepo = new Mock<IPaymentRepository>();
        _mockHotelApprovalRepo = new Mock<IHotelApprovalRequestRepository>();
        _mockUpgradeRepo = new Mock<IUpgradeRequestRepository>();

        _service = new DashboardService(
            _mockUserRepo.Object,
            _mockHotelRepo.Object,
            _mockBookingRepo.Object,
            _mockPaymentRepo.Object,
            _mockHotelApprovalRepo.Object,
            _mockUpgradeRepo.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAdminDashboardStatsAsync_ReturnsCorrectAggregation()
    {
        // Arrange
        var users = new List<User> { new User(), new User() };
        var hotels = new List<Hotel> { new Hotel() };
        var bookings = new List<Booking> { new Booking(), new Booking(), new Booking() };
        var payments = new List<Payment> 
        { 
            new Payment { Amount = 100, Status = "Success", PaidAt = DateTime.Now },
            new Payment { Amount = 50, Status = "Success", PaidAt = DateTime.Now },
            new Payment { Amount = 200, Status = "Failed", PaidAt = DateTime.Now } 
        };

        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
        _mockHotelRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(hotels);
        _mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);
        _mockPaymentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);
        _mockHotelApprovalRepo.Setup(r => r.WhereAsync(It.IsAny<System.Linq.Expressions.Expression<Func<HotelApprovalRequest, bool>>>()))
            .ReturnsAsync(new List<HotelApprovalRequest>());
        _mockUpgradeRepo.Setup(r => r.WhereAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UpgradeRequest, bool>>>()))
            .ReturnsAsync(new List<UpgradeRequest>());

        // Act
        var result = await _service.GetAdminDashboardStatsAsync();

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(result.Content);
        Assert.Equal(2, result.Content.TotalUsers);
        Assert.Equal(1, result.Content.TotalHotels);
        Assert.Equal(3, result.Content.TotalBookings);
        Assert.Equal(150, result.Content.TotalRevenue); // 100 + 50
    }

    [Fact]
    public async Task GetAdminDashboardStatsAsync_CalculatesCorrectMonthlyTrend()
    {
        // Arrange
        var currentMonth = DateTime.Now;
        var lastMonth = currentMonth.AddMonths(-1);

        var payments = new List<Payment>
        {
            new Payment { Amount = 1000, Status = "Success", PaidAt = currentMonth },
            new Payment { Amount = 500, Status = "Success", PaidAt = lastMonth }
        };

        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
        _mockHotelRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Hotel>());
        _mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Booking>());
        _mockPaymentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);
        _mockHotelApprovalRepo.Setup(r => r.WhereAsync(It.IsAny<System.Linq.Expressions.Expression<Func<HotelApprovalRequest, bool>>>()))
            .ReturnsAsync(new List<HotelApprovalRequest>());
        _mockUpgradeRepo.Setup(r => r.WhereAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UpgradeRequest, bool>>>()))
            .ReturnsAsync(new List<UpgradeRequest>());

        // Act
        var result = await _service.GetAdminDashboardStatsAsync();

        // Assert
        Assert.NotNull(result.Content);
        var currentMonthName = currentMonth.ToString("MMM yyyy", CultureInfo.InvariantCulture);
        var lastMonthName = lastMonth.ToString("MMM yyyy", CultureInfo.InvariantCulture);

        var currentTrend = result.Content.MonthlyRevenueTrend.First(t => t.MonthName == currentMonthName);
        var lastTrend = result.Content.MonthlyRevenueTrend.First(t => t.MonthName == lastMonthName);

        Assert.Equal(1000, currentTrend.Amount);
        Assert.Equal(500, lastTrend.Amount);
    }
}
