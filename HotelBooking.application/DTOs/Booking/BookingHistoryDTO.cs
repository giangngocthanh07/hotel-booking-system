namespace HotelBooking.application.DTOs.Booking;

public class BookingHistoryDTO
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string? HotelCoverImageUrl { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // For Owner view
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}
