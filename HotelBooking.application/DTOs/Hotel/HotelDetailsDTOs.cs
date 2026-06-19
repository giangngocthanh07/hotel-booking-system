namespace HotelBooking.application.DTOs.Hotel;

public class HotelDetailsDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    
    public List<string> Gallery { get; set; } = new();
    public List<AmenityDTO> Amenities { get; set; } = new();
    public List<RoomTypeDetailsDTO> RoomTypes { get; set; } = new();
    public List<ReviewDTO> RecentReviews { get; set; } = new();
}

public class RoomTypeDetailsDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int AdultCapacity { get; set; }
    public int ChildCapacity { get; set; }
    public double? AreaSqm { get; set; }
    public List<string> Images { get; set; } = new();
    public List<string> Amenities { get; set; } = new();
}

public class ReviewDTO
{
    public string UserName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
