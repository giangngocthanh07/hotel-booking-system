using System.ComponentModel.DataAnnotations;

namespace HotelBooking.application.DTOs.Booking;

public class CreatePaymentRequestDTO
{
    [Required]
    public int BookingId { get; set; }
    
    [Required]
    public string PaymentMethod { get; set; } = "VNPay";
}

public class PaymentResponseDTO
{
    public int PaymentId { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PaymentCallbackDTO
{
    public int PaymentId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}
