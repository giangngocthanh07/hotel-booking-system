public class CreateHotelRequestDTO
{
    // Thông tin cơ bản
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CityId { get; set; } = 0;

    // Ảnh
    public IFormFile? CoverFile { get; set; }   // Ảnh bìa (step 1)
    public IFormFile? MainFile { get; set; }    // Ảnh chính (step 4)
    public List<IFormFile>? SubFiles { get; set; } = new(); // 4 ảnh phụ

    // Amenities: chỉ gửi ID
    public List<int> AmenityIds { get; set; } = new();

    // Policies: chỉ gửi ID (check-in, check-out, cancellation)
    // public List<int> PolicyIds { get; set; } = new();
}