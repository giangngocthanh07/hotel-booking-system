using System.Text.Json.Serialization;
using HotelBooking.webapp.ViewModels.Hotel;

public enum ManageModuleEnum
{
    Service = 1,
    Policy = 2,
    Amenity = 3,
    RoomQuality = 4,
    UnitType = 5,
    BedType = 6,
    RoomView = 7

    // Sau này thêm Room = 3, Staff = 4...
}

// DTO dùng cho danh sách Sidebar
public class ManageTypeVM // Hoặc dùng lại BaseAdminDTO nếu bạn muốn sửa trực tiếp
{
    public int? Id { get; set; } // Null nếu là module phẳng
    public string Name { get; set; } = null!;
}
public class ManageMenuResultVM
{
    // Danh sách các loại (VD: Standard, VIP...)
    public List<ManageTypeVM> Types { get; set; } = new();

    // ID mặc định (Backend gợi ý - tuỳ chọn)
    public int? DefaultSelectedId { get; set; }
}

// T là kiểu dữ liệu của item (VD: ServiceBaseDTO, PolicyDTO...)
public class PagedManageResult<T> : PagedResult<T>
{
    public int? SelectedTypeId { get; set; }
}