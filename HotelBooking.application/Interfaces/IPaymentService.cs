using HotelBooking.application.DTOs.Booking;

namespace HotelBooking.application.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PaymentResponseDTO>> CreatePaymentUrlAsync(CreatePaymentRequestDTO request, int userId);
    Task<ApiResponse<bool>> ProcessPaymentCallbackAsync(PaymentCallbackDTO callback);
}
