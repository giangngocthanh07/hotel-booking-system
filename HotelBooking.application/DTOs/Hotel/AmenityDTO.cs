using System.ComponentModel.DataAnnotations;

public enum AmenityTypeEnum
{
    InRoom = 1,
    BathRoom = 2,
    SafetySecurity = 3,
    General = 4,
    Nearby = 5,
    FoodAndDrink = 6,
}

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
// 1. DTO dùng cho TẠO MỚI (Cần TypeId) -> Kế thừa Base để lấy Name, Description
public class AmenityCreateDTO : BaseCreateOrUpdateAdminDTO
{
    [Required(ErrorMessage = "Loại tiện nghi không được để trống")]
    public int TypeId { get; set; }
}

// 2. DTO dùng cho CẬP NHẬT (Không có TypeId) -> Sạch bóng trên Swagger!
public class AmenityUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    // Rỗng, chỉ lấy Name và Description từ Base
}
