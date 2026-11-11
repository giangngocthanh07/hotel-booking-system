namespace HotelBooking.application.DTOs.Hotel.Owner;

public class OwnerHotelDetailsDTO : OwnerHotelSummaryDTO
{
    public string? Description { get; set; }
    public int PropertyTypeId { get; set; }
    public string PropertyTypeName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public int ProvinceId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public int WardId { get; set; }
    public string WardName { get; set; } = string.Empty;
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public int? StarRating { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string TaxCode { get; set; } = string.Empty;
}
