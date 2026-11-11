namespace HotelBooking.application.DTOs.Hotel.Owner.Images;

public class OwnerHotelMediaDTO
{
    public int HotelId { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<OwnerHotelImageDTO> Gallery { get; set; } = new();
}
