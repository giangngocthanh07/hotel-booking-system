namespace HotelBooking.webapp.ViewModels.Hotel;

public class HotelDetailsVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    
    public List<string> Gallery { get; set; } = new();
    public List<AmenityVM> Amenities { get; set; } = new();
    public List<RoomTypeDetailsVM> RoomTypes { get; set; } = new();
    public List<ReviewVM> RecentReviews { get; set; } = new();
}

public class RoomTypeDetailsVM
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

public class ReviewVM
{
    public string UserName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OwnerResponse { get; set; }
    public DateTime? OwnerResponseAt { get; set; }
}

public class AmenityVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
