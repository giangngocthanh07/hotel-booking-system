public class CreateHotelDTO
{
    // Basic information
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CityId { get; set; } = 0;

    // Images
    public UploadFileDTO? CoverFile { get; set; }   // Cover image (step 1)
    public UploadFileDTO? MainFile { get; set; }    // Main image (step 4)
    public List<UploadFileDTO>? SubFiles { get; set; } = new(); // 4 sub images

    // Amenities: only send ID
    public List<int> AmenityIds { get; set; } = new();

    // Policies: only send ID
    public List<int> PolicyIds { get; set; } = new();

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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