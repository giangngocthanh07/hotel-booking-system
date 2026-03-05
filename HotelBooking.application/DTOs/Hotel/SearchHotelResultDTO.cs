public class SearchHotelResultDTO
{
    public int Id { get; set; }                // h.Id
    public string Name { get; set; } = string.Empty;           // h.Name
    public string Address { get; set; } = string.Empty;     // h.Address
    public string? Description { get; set; } = string.Empty; // h.Description
    public string CityName { get; set; } = string.Empty;       // c.Name
    public string CountryName { get; set; } = string.Empty;   // co.Name
    public string CoverImageUrl { get; set; } = string.Empty; // h.CoverImageUrl

    // Price casted to DECIMAL(18,2) in SP → DTO uses decimal
    public decimal PriceFrom { get; set; }

    // Casted to INT → DTO uses int
    public int MaxAdultCapacity { get; set; }
    public int MaxChildCapacity { get; set; }

    // AVG casted to DECIMAL(18,2) → DTO uses decimal
    public decimal AvgRating { get; set; }

    // COUNT casted to INT → DTO uses int
    public int ReviewCount { get; set; }
    public int AvailableRooms { get; set; }
}