using System.ComponentModel.DataAnnotations;

public class AmenityTypeVM : BaseAdminVM
{
    public string? IconClass { get; set; }
    public string? IconColor { get; set; }
}

public class AmenityVM : BaseAdminVM
{
    public int TypeId { get; set; }
}

// 2. VM Tạo mới (Input - CÓ TypeId)
public class AmenityCreateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Loại tiện ích không được để trống!")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại tiện ích")] // Validate số > 0 (cho Dropdown)
    public int TypeId { get; set; }
}

// 3. VM Cập nhật (Input - KHÔNG TypeId)
public class AmenityUpdateVM : BaseCreateOrUpdateAdminVM
{
    // Rỗng, chỉ lấy Name và Description từ Base
    // Việc không có TypeId ở đây giúp Form Edit không bao giờ vô tình gửi TypeId lên Server
}
