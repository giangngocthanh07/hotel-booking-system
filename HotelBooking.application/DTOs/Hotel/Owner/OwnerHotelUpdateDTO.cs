namespace HotelBooking.application.DTOs.Hotel.Owner;

// FR-CAT-002/003: an allowlist prevents updates to ownership, approval, operational or legal fields.
public class OwnerHotelUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public int ProvinceId { get; set; }
    public int WardId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
