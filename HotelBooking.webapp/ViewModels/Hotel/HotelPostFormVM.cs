using System.ComponentModel.DataAnnotations;

public class HotelPostFormVM
{
    // 1. Basic Info
    [Required(ErrorMessage = "Vui lòng điền tên khách sạn!")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng điền địa chỉ!")]
    [MinLength(10, ErrorMessage = "Địa chỉ phải có ít nhất 10 ký tự!")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn thành phố!")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn thành phố!")]
    public int CityId { get; set; }

    [MinLength(20, ErrorMessage = "Mô tả phải có ít nhất 20 ký tự!")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn ảnh bìa hợp lệ!")]
    public UploadFileVM? CoverFile { get; set; }

    // 2. Amenities
    public List<int> AmenityIds { get; set; } = new();

    // 3. Policies
    public List<int> PolicyIds { get; set; } = new();

    // 4. Hotel Images
    public UploadFileVM? MainFile { get; set; }
    public List<UploadFileVM> SubFiles { get; set; } = new();

}