using HotelBooking.application.DTOs.Booking;

namespace HotelBooking.application.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<BookingResponseDTO>> CreateBookingAsync(BookingRequestDTO request, int userId);
    Task<ApiResponse<IEnumerable<BookingHistoryDTO>>> GetGuestBookingsAsync(int userId, string? status);
    Task<ApiResponse<IEnumerable<BookingHistoryDTO>>> GetOwnerBookingsAsync(int ownerId, string? status, string? searchTerm);
}
