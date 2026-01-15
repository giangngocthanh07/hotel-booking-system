using System.ComponentModel.DataAnnotations;

public class AmenityTypeDTO : BaseAdminDTO
{
    // Parse từ Additional JSON

    // [Required(ErrorMessage = "Icon class is required")]
    public string? IconClass { get; set; }
    public string? IconColor { get; set; }
}

// 1. DTO hiển thị
public class AmenityDTO : BaseAdminDTO
{
    public int TypeId { get; set; }
}

// 2. DTO Thêm/Sửa
public class AmenityCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public int TypeId { get; set; }
}
