namespace HotelBooking.application.DTOs.Booking;

public class BookingRequestDTO
{
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; } = 1;
    public string? Notes { get; set; }
}

public class BookingResponseDTO
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}
