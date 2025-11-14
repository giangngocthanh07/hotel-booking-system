public class CreateHotelDTO
{
    // Thông tin cơ bản
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";

    // Chính sách: FE gửi 3 Id policy
    public int SelectedCheckInId { get; set; }
    public int SelectedCheckOutId { get; set; }
    public int SelectedCancellationId { get; set; }

    // Hoặc gọn hơn: chỉ cần 1 list PolicyIds
    public List<int> PolicyIds
    {
        get
        {
            var ids = new List<int>();
            if (SelectedCheckInId != 0) ids.Add(SelectedCheckInId);
            if (SelectedCheckOutId != 0) ids.Add(SelectedCheckOutId);
            if (SelectedCancellationId != 0) ids.Add(SelectedCancellationId);
            return ids;
        }
    }

    // Tiện ích (FE gửi danh sách Id tiện ích được chọn)
    public List<int> AmenityIds { get; set; } = new();

    // Ảnh
    public IFormFile? CoverFile { get; set; }        // 1 ảnh bìa
    public IFormFile? MainFile { get; set; }         // 1 ảnh chính
    public List<IFormFile>? SubFiles { get; set; }   // 4 ảnh phụ
}