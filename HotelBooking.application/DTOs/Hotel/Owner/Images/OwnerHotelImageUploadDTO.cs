namespace HotelBooking.application.DTOs.Hotel.Owner.Images;

public class OwnerHotelImageUploadDTO
{
    public int HotelId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int? ImageId { get; set; }
    public int? DisplayOrder { get; set; }
}
