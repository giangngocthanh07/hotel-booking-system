namespace HotelBooking.infrastructure.Models;

public class SearchHotelResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public decimal PriceFrom { get; set; }
    public int MaxAdultCapacity { get; set; }
    public int MaxChildCapacity { get; set; }
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public int AvailableRooms { get; set; }
}