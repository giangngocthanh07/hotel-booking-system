using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Interfaces;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HotelBooking.application.Services.Domains.HotelManagement;

public class ReviewAdditionalData
{
    public string? OwnerResponse { get; set; }
    public DateTime? OwnerResponseAt { get; set; }
    public string? AdminRemark { get; set; }
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IUnitOfWork _dbu;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IReviewRepository reviewRepo, IBookingRepository bookingRepo, IHotelRepository hotelRepo, IUnitOfWork dbu, ILogger<ReviewService> logger)
    {
        _reviewRepo = reviewRepo;
        _bookingRepo = bookingRepo;
        _hotelRepo = hotelRepo;
        _dbu = dbu;
        _logger = logger;
    }

    public async Task<ApiResponse<ReviewDetailDTO>> SubmitReviewAsync(ReviewRequestDTO request, int userId)
    {
        try
        {
            // 1. Validate booking ownership
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null || booking.CustomerId != userId)
            {
                return ResponseFactory.Failure<ReviewDetailDTO>(StatusCodeResponse.NotFound, "Booking not found.");
            }

            // 2. Check if already reviewed
            var existingReview = await _reviewRepo.AnyAsync(r => r.HotelId == request.HotelId && r.CustomerId == userId);
            if (existingReview)
            {
                return ResponseFactory.Failure<ReviewDetailDTO>(StatusCodeResponse.Conflict, "You have already reviewed this hotel.");
            }

            await _dbu.BeginTransactionAsync();

            var review = new Review
            {
                HotelId = request.HotelId,
                CustomerId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _reviewRepo.AddAsync(review);
            await _dbu.SaveChangesAsync();
            await _dbu.CommitTransactionAsync();

            var content = new ReviewDetailDTO
            {
                Id = review.Id,
                UserName = "Me", 
                Rating = review.Rating ?? 0,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt ?? DateTime.UtcNow
            };

            return ResponseFactory.Success(content, "Review submitted successfully.");
        }
        catch (Exception ex)
        {
            await _dbu.RollBackTransactionAsync();
            _logger.LogError(ex, "Error submitting review");
            return ResponseFactory.ServerError<ReviewDetailDTO>();
        }
    }

    public async Task<ApiResponse<IEnumerable<ReviewDetailDTO>>> GetHotelReviewsAsync(int hotelId)
    {
        try
        {
            var reviews = await _reviewRepo.WhereAsync(r => r.HotelId == hotelId && r.IsDeleted != true);
            
            var content = reviews.Select(r => {
                var additional = !string.IsNullOrEmpty(r.Additional) 
                    ? JsonSerializer.Deserialize<ReviewAdditionalData>(r.Additional) 
                    : new ReviewAdditionalData();

                return new ReviewDetailDTO
                {
                    Id = r.Id,
                    UserName = r.Customer?.FullName ?? "Anonymous",
                    Rating = r.Rating ?? 0,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt ?? DateTime.UtcNow,
                    OwnerResponse = additional?.OwnerResponse,
                    OwnerResponseAt = additional?.OwnerResponseAt,
                    IsHidden = r.IsDeleted == true
                };
            }).OrderByDescending(r => r.CreatedAt);

            return ResponseFactory.Success(content.AsEnumerable(), "Reviews retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for hotel {HotelId}", hotelId);
            return ResponseFactory.ServerError<IEnumerable<ReviewDetailDTO>>();
        }
    }

    public async Task<ApiResponse<bool>> ReplyToReviewAsync(ReviewReplyRequestDTO request, int ownerId)
    {
        try
        {
            var review = await _reviewRepo.GetByIdAsync(request.ReviewId);
            if (review == null) return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, "Review not found.");

            // Security: Check if the hotel belongs to this owner
            var hotel = await _hotelRepo.GetByIdAsync(review.HotelId);
            if (hotel == null || hotel.OwnerId != ownerId)
            {
                return ResponseFactory.Failure<bool>(StatusCodeResponse.Forbidden, "You do not have permission to reply to this review.");
            }

            var additional = !string.IsNullOrEmpty(review.Additional)
                ? JsonSerializer.Deserialize<ReviewAdditionalData>(review.Additional)
                : new ReviewAdditionalData();

            additional!.OwnerResponse = request.ReplyText;
            additional.OwnerResponseAt = DateTime.UtcNow;

            review.Additional = JsonSerializer.Serialize(additional);
            await _reviewRepo.UpdateAsync(review);
            await _dbu.SaveChangesAsync();

            return ResponseFactory.Success(true, "Reply posted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to review {ReviewId}", request.ReviewId);
            return ResponseFactory.ServerError<bool>();
        }
    }

    public async Task<ApiResponse<bool>> HideReviewAsync(ReviewModerationRequestDTO request)
    {
        try
        {
            var review = await _reviewRepo.GetByIdAsync(request.ReviewId);
            if (review == null) return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, "Review not found.");

            var additional = !string.IsNullOrEmpty(review.Additional)
                ? JsonSerializer.Deserialize<ReviewAdditionalData>(review.Additional)
                : new ReviewAdditionalData();

            additional!.AdminRemark = request.Reason;

            review.Additional = JsonSerializer.Serialize(additional);
            review.IsDeleted = true; // Soft delete / Hide

            await _reviewRepo.UpdateAsync(review);
            await _dbu.SaveChangesAsync();

            return ResponseFactory.Success(true, "Review hidden by administrator.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating review {ReviewId}", request.ReviewId);
            return ResponseFactory.ServerError<bool>();
        }
    }
}
