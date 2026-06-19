using Moq;
using HotelBooking.application.Services.Domains.BookingManagement;
using HotelBooking.application.DTOs.Booking;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.test.UnitTests.Services.BookingManagement;

public class PaymentServiceTests : BaseServiceTest<PaymentService>
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _mockPaymentRepo = new Mock<IPaymentRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();

        _service = new PaymentService(
            _mockPaymentRepo.Object,
            _mockBookingRepo.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task ProcessPaymentCallbackAsync_Success_UpdatesBookingStatus()
    {
        // Arrange
        var callback = new PaymentCallbackDTO
        {
            PaymentId = 1,
            TransactionId = "GATEWAY-123",
            IsSuccess = true
        };

        var payment = new Payment { Id = 1, BookingId = 100 };
        var booking = new Booking { Id = 100, Status = "Pending" };

        _mockPaymentRepo.Setup(r => r.GetByIdAsync(callback.PaymentId)).ReturnsAsync(payment);
        _mockBookingRepo.Setup(r => r.GetByIdAsync(payment.BookingId)).ReturnsAsync(booking);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.ProcessPaymentCallbackAsync(callback);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("Success", payment.Status);
        Assert.Equal("Confirmed", booking.Status);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }
}
