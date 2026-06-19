using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewDetailDTO>> SubmitReviewAsync(ReviewRequestDTO request, int userId);
    Task<ApiResponse<IEnumerable<ReviewDetailDTO>>> GetHotelReviewsAsync(int hotelId);
    Task<ApiResponse<bool>> ReplyToReviewAsync(ReviewReplyRequestDTO request, int ownerId);
    Task<ApiResponse<bool>> HideReviewAsync(ReviewModerationRequestDTO request);
}
