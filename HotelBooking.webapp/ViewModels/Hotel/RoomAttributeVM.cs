using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

public enum RoomAttributeType
{
    UnitType = 1,
    BedType = 2,
    RoomView = 3,
    RoomQuality = 4
}

public class GetRoomAttributeRequest
{
    public RoomAttributeType Type { get; set; } = RoomAttributeType.UnitType; // Mặc định
    public PagingRequest Paging { get; set; } = new PagingRequest();
    public int? TypeId { get; set; } // Chỉ dùng cho RoomQuality
}

// Báo hiệu đây là lớp đa hình
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
// Khai báo các con và đặt tên định danh (discriminator) cho chúng
[JsonDerivedType(typeof(UnitTypeVM), typeDiscriminator: "unit")]
[JsonDerivedType(typeof(RoomViewVM), typeDiscriminator: "roomview")]
[JsonDerivedType(typeof(BedTypeVM), typeDiscriminator: "bed")]
[JsonDerivedType(typeof(RoomQualityVM), typeDiscriminator: "roomquality")]
public abstract class RoomAttributeVM : BaseAdminVM
{
    public abstract RoomAttributeType AttributeType { get; }
}

public class UnitTypeVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.UnitType; // Tự động gán số 1
    public bool? IsEntirePlace { get; set; } = false;
}

// 2. Create
public class UnitTypeCreateVM : BaseCreateOrUpdateAdminVM
{
    public bool? IsEntirePlace { get; set; } = false;
}

// 3. Update
public class UnitTypeUpdateVM : BaseCreateOrUpdateAdminVM
{
    public bool? IsEntirePlace { get; set; }
}

public class RoomViewVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomView; // Tự động gán số 3
}

// 2. Create
public class RoomViewCreateVM : BaseCreateOrUpdateAdminVM
{
}

// 3. Update
public class RoomViewUpdateVM : BaseCreateOrUpdateAdminVM
{
}

public class BedTypeVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.BedType; // Tự động gán số 2
    [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0!")]
    public int? DefaultCapacity { get; set; } = 1;

    // --- CÁC TRƯỜNG THÔNG SỐ KỸ THUẬT BỔ SUNG ---

    // Kích thước chiều ngang tối thiểu (Inch)
    public double MinWidth { get; set; }

    // Kích thước chiều ngang tối đa (Inch)
    public double MaxWidth { get; set; }

    // Thêm set để hỗ trợ Binding từ UI
    private bool? _isVaryingSize;
    public bool IsVaryingSize
    {
        // Logic hiển thị: Nếu chưa tác động thì tính dựa trên MinWidth, 
        // nếu đã gạt switch thì lấy giá trị của switch.
        get => _isVaryingSize ?? (MinWidth <= 0 && MaxWidth <= 0);
        set
        {
            _isVaryingSize = value;
            if (value) // Nếu người dùng bật switch "Đa dạng kích thước"
            {
                MinWidth = 0;
                MaxWidth = 0;
            }
        }
    }

    public string SizeDisplay => IsVaryingSize ? "Đa dạng" : $"{MinWidth}\" - {MaxWidth}\"";
}

// 2. Create
public class BedTypeCreateVM : BaseCreateOrUpdateAdminVM
{
    [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0!")]
    public int? DefaultCapacity { get; set; } = 1;

    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }

    // Hỗ trợ Binding cho Switch
    private bool? _isVaryingSize;
    public bool IsVaryingSize
    {
        // Nếu người dùng chưa gạt switch (_isVaryingSize == null), 
        // ta tính toán dựa trên dữ liệu hiện có (dùng khi Edit).
        // Nếu đã gạt switch, ta tin tưởng hoàn toàn vào giá trị của switch.
        get => _isVaryingSize ?? (MinWidth <= 0 && MaxWidth <= 0);
        set
        {
            _isVaryingSize = value;
            if (value)
            {
                MinWidth = 0;
                MaxWidth = 0;
            }
        }
    }
}

// 3. Update (Giống Create nhưng tách ra để dễ mở rộng sau này)
public class BedTypeUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0!")]
    public int? DefaultCapacity { get; set; } = 1;

    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }

    private bool? _isVaryingSize;
    public bool IsVaryingSize
    {
        // Nếu người dùng chưa gạt switch (_isVaryingSize == null), 
        // ta tính toán dựa trên dữ liệu hiện có (dùng khi Edit).
        // Nếu đã gạt switch, ta tin tưởng hoàn toàn vào giá trị của switch.
        get => _isVaryingSize ?? (MinWidth <= 0 && MaxWidth <= 0);
        set
        {
            _isVaryingSize = value;
            if (value)
            {
                MinWidth = 0;
                MaxWidth = 0;
            }
        }
    }
}

public class BedTypeAdditionalData
{
    // Kích thước chiều ngang (Inch)
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }

    // Cờ đánh dấu nếu kích thước không cố định
    public bool IsVaryingSize => MinWidth <= 0 && MaxWidth <= 0;
}

public class RoomQualityGroupVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomQuality; // Tự động gán số 4
    public int? SortOrder { get; set; } = 0;

}

public class RoomQualityVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomQuality; // Tự động gán số 4
    public int? SortOrder { get; set; } = 0;

    [Required(ErrorMessage = "Loại chất lượng phòng không được để trống!")]
    public int TypeId { get; set; }
}

// 2. Create
public class RoomQualityCreateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Vui lòng chọn nhóm hạng phòng!")]
    public int TypeId { get; set; }

    [Range(0, 100, ErrorMessage = "Thứ tự ưu tiên từ 0-100")]
    public int SortOrder { get; set; } = 0;
}

// 3. Update (Không có TypeId)
public class RoomQualityUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Range(0, 100, ErrorMessage = "Thứ tự ưu tiên từ 0-100")]
    public int? SortOrder { get; set; } = 0;
}


// Dùng cho Bed Type (Cần hứng thêm DefaultCapacity từ Form)
public class BedTypeCreateOrUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0!")]
    public int DefaultCapacity { get; set; } = 1;


    // Kích thước chiều ngang tối thiểu (Inch)
    public double MinWidth { get; set; }

    // Kích thước chiều ngang tối đa (Inch)
    public double MaxWidth { get; set; }

    // Hỗ trợ Binding 2 chiều cho Switch trên Sidebar
    private bool? _isVaryingSize;
    public bool IsVaryingSize
    {
        // Nếu người dùng chưa gạt switch (_isVaryingSize == null), 
        // ta tính toán dựa trên dữ liệu hiện có (dùng khi Edit).
        // Nếu đã gạt switch, ta tin tưởng hoàn toàn vào giá trị của switch.
        get => _isVaryingSize ?? (MinWidth <= 0 && MaxWidth <= 0);
        set
        {
            _isVaryingSize = value;
            if (value)
            {
                MinWidth = 0;
                MaxWidth = 0;
            }
        }
    }
}


