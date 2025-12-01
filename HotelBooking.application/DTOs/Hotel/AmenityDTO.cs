using System.ComponentModel.DataAnnotations;

// 1. DTO hiển thị
public class AmenityDTO : BaseAdminDTO
{
    // Parse từ Additional JSON

    [Required(ErrorMessage = "Icon class is required")]
    public string IconClass { get; set; } = null!;
    public string? IconColor { get; set; } = "blue";
}

// 2. DTO Thêm/Sửa
public class AmenityCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public string IconClass { get; set; } = string.Empty;
    public string? IconColor { get; set; } = "blue";
}
