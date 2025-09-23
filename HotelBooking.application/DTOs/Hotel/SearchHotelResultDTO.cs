public class SearchHotelResultDTO
{
    public int Id { get; set; }                // h.Id
    public string Name { get; set; }           // h.Name
    public string Address { get; set; }        // h.Address
    public string CityName { get; set; }       // c.Name
    public string CountryName { get; set; }    // co.Name
    public string CoverImageUrl { get; set; }  // h.CoverImageUrl

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