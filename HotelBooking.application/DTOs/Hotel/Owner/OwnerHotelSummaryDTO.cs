namespace HotelBooking.application.DTOs.Hotel.Owner;

public class OwnerHotelSummaryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? Status { get; set; }
    public bool? IsVerified { get; set; }
    public DateTime? CreatedAt { get; set; }
}
