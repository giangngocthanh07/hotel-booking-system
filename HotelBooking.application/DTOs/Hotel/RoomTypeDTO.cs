namespace HotelBooking.application.DTOs.Hotel;

public class RoomTypeCreateDTO
{
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsDeleted { get; set; } = false;
    public decimal PricePerNight { get; set; }
    public int AdultCapacity { get; set; }
    public int ChildCapacity { get; set; }
    public int UnitTypeId { get; set; }
    public int? QualityId { get; set; }
    public int? RoomViewId { get; set; }
    public bool IsPrivateBathroom { get; set; } = true;
    public bool HasBalcony { get; set; } = false;
    public bool HasTerrace { get; set; } = false;
    public bool CanAddExtraBed { get; set; } = false;
    public int? MaxExtraBeds { get; set; }
    public float? AreaSqm { get; set; } // in square meters
    public bool? IsSmokingAllowed { get; set; } = false;
    public int TotalRooms { get; set; } = 1; // Total number of rooms of this type available in the hotel

    // Optional: List of BedType configurations for this RoomType
    public List<BedTypeConfigDTO> BedTypes { get; set; } = new();
}

public class BedTypeConfigDTO
{
    public int BedTypeId { get; set; }
    public int Quantity { get; set; }
}

public class BedTypeNameDTO
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class RoomNameSuggestionRequest
{
    // Input parameters for suggesting room names based on attributes
    public int UnitTypeId { get; set; }
    public int? QualityId { get; set; }
    public int? RoomViewId { get; set; }

    // "2 Adults, 1 Child", "4 Adults"
    public int AdultCapacity { get; set; }
    public int ChildrenCapacity { get; set; }

    // "with Balcony", "with Private Bathroom"
    public bool IsPrivateBathroom { get; set; } = true;
    public bool HasBalcony { get; set; } = false;
    public bool HasTerrace { get; set; } = false;

    // Extra bed configuration
    public bool CanAddExtraBeds { get; set; } = false;
    public int? MaxExtraBeds { get; set; }

    // "Double Room", "Twin Room", "King Room"
    public List<BedTypeConfigDTO> BedTypes { get; set; } = new();
}

public class RoomAttributeNamesDTO
{
    public string UnitTypeName { get; set; } = string.Empty;
    public string? QualityName { get; set; }       // nullable  
    public string? RoomViewName { get; set; }       // nullable
    public List<BedTypeNameDTO> BedTypeNames { get; set; } = new();
}