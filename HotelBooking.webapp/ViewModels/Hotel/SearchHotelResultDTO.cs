namespace HotelBooking.webapp.ViewModels.Hotel;

public class SearchHotelResultDTO
{
    public int Id { get; set; }                // h.Id
    public string Name { get; set; } = string.Empty;           // h.Name
    public string Address { get; set; } = string.Empty;       // h.Address
    public string CityName { get; set; } = string.Empty;      // c.Name
    public string CountryName { get; set; } = string.Empty;   // co.Name
    public string CoverImageUrl { get; set; } = string.Empty; // h.CoverImageUrl

    // Giá ép kiểu DECIMAL(18,2) trong SP → DTO để decimal
    public decimal PriceFrom { get; set; }

    // Ép kiểu INT → DTO để int
    public int MaxAdultCapacity { get; set; }
    public int MaxChildCapacity { get; set; }

    // AVG ép về DECIMAL(18,2) → DTO để decimal
    public decimal AvgRating { get; set; }

    // COUNT ép về INT → DTO để int
    public int ReviewCount { get; set; }
    public int AvailableRooms { get; set; }
}