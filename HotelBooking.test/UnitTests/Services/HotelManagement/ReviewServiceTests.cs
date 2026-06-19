using Moq;
using HotelBooking.application.Services.Domains.HotelManagement;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.infrastructure.Models;
using HotelBooking.application.Interfaces;
using System.Linq.Expressions;

namespace HotelBooking.test.UnitTests.Services.HotelManagement;

public class ReviewServiceTests : BaseServiceTest<ReviewService>
{
    private readonly Mock<IReviewRepository> _mockReviewRepo;
    private readonly Mock<IBookingRepository> _mockBookingRepo;
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _mockReviewRepo = new Mock<IReviewRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockHotelRepo = new Mock<IHotelRepository>();

        _service = new ReviewService(
            _mockReviewRepo.Object,
            _mockBookingRepo.Object,
            _mockHotelRepo.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task SubmitReviewAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        int userId = 1;
        var request = new ReviewRequestDTO
        {
            BookingId = 1,
            HotelId = 1,
            Rating = 5,
            Comment = "Great stay!"
        };

        var booking = new Booking { Id = 1, CustomerId = userId, HotelId = 1 };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(request.BookingId)).ReturnsAsync(booking);
        _mockReviewRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(false);

        // Act
        var result = await _service.SubmitReviewAsync(request, userId);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        _mockReviewRepo.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitReviewAsync_AlreadyReviewed_ReturnsConflict()
    {
        // Arrange
        int userId = 1;
        var request = new ReviewRequestDTO { BookingId = 1, HotelId = 1, Rating = 5 };

        var booking = new Booking { Id = 1, CustomerId = userId };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(request.BookingId)).ReturnsAsync(booking);
        _mockReviewRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(true);

        // Act
        var result = await _service.SubmitReviewAsync(request, userId);

        // Assert
        Assert.Equal(StatusCodeResponse.Conflict, result.StatusCode);
    }

    [Fact]
    public async Task ReplyToReviewAsync_OwnerOwnsHotel_ReturnsSuccess()
    {
        // Arrange
        int ownerId = 1;
        var request = new ReviewReplyRequestDTO { ReviewId = 1, ReplyText = "Thanks!" };
        var review = new Review { Id = 1, HotelId = 10 };
        var hotel = new Hotel { Id = 10, OwnerId = ownerId };

        _mockReviewRepo.Setup(r => r.GetByIdAsync(request.ReviewId)).ReturnsAsync(review);
        _mockHotelRepo.Setup(r => r.GetByIdAsync(review.HotelId)).ReturnsAsync(hotel);

        // Act
        var result = await _service.ReplyToReviewAsync(request, ownerId);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        _mockReviewRepo.Verify(r => r.UpdateAsync(It.IsAny<Review>()), Times.Once);
    }

    [Fact]
    public async Task ReplyToReviewAsync_NotOwner_ReturnsForbidden()
    {
        // Arrange
        int ownerId = 1;
        var request = new ReviewReplyRequestDTO { ReviewId = 1, ReplyText = "Thanks!" };
        var review = new Review { Id = 1, HotelId = 10 };
        var hotel = new Hotel { Id = 10, OwnerId = 999 }; // Different owner

        _mockReviewRepo.Setup(r => r.GetByIdAsync(request.ReviewId)).ReturnsAsync(review);
        _mockHotelRepo.Setup(r => r.GetByIdAsync(review.HotelId)).ReturnsAsync(hotel);

        // Act
        var result = await _service.ReplyToReviewAsync(request, ownerId);

        // Assert
        Assert.Equal(StatusCodeResponse.Forbidden, result.StatusCode);
    }
}
