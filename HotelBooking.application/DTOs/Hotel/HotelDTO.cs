namespace HotelBooking.application.DTOs.Hotel;

public static class HotelStatus
{
    public const string Suspended = "Suspended";
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Deleted = "Deleted";
}

// Hotel Registration Form
public class HotelRegistrationDTO
{
    // Basic information
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PropertyTypeId { get; set; }
    public int? StarRating { get; set; }

    // 2. Contact
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;

    // 3. Location
    public string Address { get; set; } = string.Empty;
    public int CountryId { get; set; } = 4; // Default is 4 - Vietnam
    public int ProvinceId { get; set; } = 0;
    public int WardId { get; set; } = 0;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // 4. Legal / Approval Info
    public string TaxCode { get; set; } = string.Empty;

    // Save URL file from AWS S3
    public string BusinessLicenseUrl { get; set; } = string.Empty;

}

public class HotelAdditionalInfo
{
    public int? StarRating { get; set; }
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public string BusinessLicenseUrl { get; set; } = string.Empty;
}


public class CreateHotelRequestDTO
{
    // Basic information
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CityId { get; set; } = 0;

    // Images
    public IFormFile? CoverFile { get; set; }   // Cover image (step 1)
    public IFormFile? MainFile { get; set; }    // Main image (step 4)
    public List<IFormFile>? SubFiles { get; set; } = new(); // 4 sub images

    // Amenities: only send ID
    public List<int> AmenityIds { get; set; } = new();

    // Policies: only send ID
    public List<int> PolicyIds { get; set; } = new();
}

public class CreateHotelResponseDTO
{
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
}