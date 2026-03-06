namespace HotelBooking.webapp.ViewModels.Hotel;

/// <summary>
/// Data Transfer Object representing the summarized hotel information 
/// returned from a search query.
/// </summary>
public class SearchHotelResultDTO
{
    // --- BASIC HOTEL INFORMATION ---
    public int Id { get; set; }                             // maps to h.Id
    public string Name { get; set; } = string.Empty;        // maps to h.Name
    public string Address { get; set; } = string.Empty;     // maps to h.Address
    public string CityName { get; set; } = string.Empty;    // maps to c.Name
    public string CountryName { get; set; } = string.Empty; // maps to co.Name
    public string CoverImageUrl { get; set; } = string.Empty; // maps to h.CoverImageUrl

    // --- PRICING & CAPACITY ---
    // Price cast as DECIMAL(18,2) in SP → mapped to decimal in DTO
    public decimal PriceFrom { get; set; }

    // Capacities cast as INT in SP → mapped to int in DTO
    public int MaxAdultCapacity { get; set; }
    public int MaxChildCapacity { get; set; }

    // --- RATINGS & AVAILABILITY ---
    // Average rating cast to DECIMAL(18,2) in SP → mapped to decimal in DTO
    public decimal AvgRating { get; set; }

    // Counts cast as INT in SP → mapped to int in DTO
    public int ReviewCount { get; set; }
    public int AvailableRooms { get; set; }
}