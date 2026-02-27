using System.Text.Json.Serialization;

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
public class ManageTypeDTO // Hoặc dùng lại BaseAdminDTO nếu bạn muốn sửa trực tiếp
{
    public int? Id { get; set; } // Null nếu là module phẳng
    public string Name { get; set; } = string.Empty;
}

// DTO dùng để hứng tham số đầu vào cho API lấy Menu
public class ManageMenuRequest
{
    // Đây là tham số chính mà ta muốn validate
    public ManageModuleEnum Module { get; set; }
}

// Result cho API: /get-types/{module}
public class ManageMenuResult
{
    // Danh sách các loại (VD: Standard, VIP...)
    public List<ManageTypeDTO> Types { get; set; } = new();

    // (Tùy chọn) Gợi ý ID mặc định nếu muốn BE quyết định logic default
    // Nếu bạn để FE tự chọn cái đầu tiên thì không cần dòng này.
    public int? DefaultSelectedId { get; set; }
}

// T là kiểu dữ liệu của item (VD: ServiceBaseDTO, PolicyDTO...)
public class PagedManageResult<T> : PagedResult<T>
{
    public int? SelectedTypeId { get; set; }

    public PagedManageResult(List<T> items, int count, int pageIndex, int pageSize, int? selectedType)
        : base(items, count, pageIndex, pageSize)
    {
        SelectedTypeId = selectedType;
    }
}
