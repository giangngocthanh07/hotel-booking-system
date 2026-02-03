using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class GetRoomAttributeRequest
{
    public RoomAttributeType Type { get; set; }
    public PagingRequest Paging { get; set; } = new(); // Init sẵn để tránh null
    public int? TypeId { get; set; }
}

public enum RoomAttributeType
{
    UnitType = 1,
    BedType = 2,
    RoomView = 3,
    RoomQuality = 4
}

// Báo hiệu đây là lớp đa hình
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
// Khai báo các con và đặt tên định danh (discriminator) cho chúng
[JsonDerivedType(typeof(UnitTypeDTO), typeDiscriminator: "unit")]
[JsonDerivedType(typeof(RoomViewDTO), typeDiscriminator: "roomview")]
[JsonDerivedType(typeof(BedTypeDTO), typeDiscriminator: "bed")]
[JsonDerivedType(typeof(RoomQualityDTO), typeDiscriminator: "roomquality")]
public abstract class RoomAttributeDTO : BaseAdminDTO
{
    public abstract RoomAttributeType AttributeType { get; }
}

public class UnitTypeDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.UnitType; // Tự động gán số 1
    public bool? IsEntirePlace { get; set; } = false;
}

public class RoomViewDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomView; // Tự động gán số 3
}

public class BedTypeDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.BedType; // Tự động gán số 2
    public int? DefaultCapacity { get; set; } = 1;

    // --- CÁC TRƯỜNG THÔNG SỐ KỸ THUẬT BỔ SUNG ---

    // Kích thước chiều ngang tối thiểu (Inch)
    public double MinWidth { get; set; }

    // Kích thước chiều ngang tối đa (Inch)
    public double MaxWidth { get; set; }

    // Cờ logic để hiển thị Badge "Varying Size" hoặc khoảng số trên UI
    public bool IsVaryingSize => MinWidth <= 0 && MaxWidth <= 0;

    // Helper property để hiển thị text cho đẹp (VD: "39 - 75 inch" hoặc "Đa dạng")
    public string SizeDisplay => IsVaryingSize
        ? "Kích thước đa dạng"
        : $"{MinWidth}\" - {MaxWidth}\"";
}

public class BedTypeAdditionalData
{
    // Kích thước chiều ngang (Inch)
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }

    // Cờ đánh dấu nếu kích thước không cố định
    public bool IsVaryingSize => MinWidth <= 0 && MaxWidth <= 0;
}

public class RoomQualityGroupDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomQuality; // Tự động gán số 4
    public int? SortOrder { get; set; } = 0;

}

public class RoomQualityDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomQuality; // Tự động gán số 4
    public int? SortOrder { get; set; } = 0;
    public int TypeId { get; set; }
}

// CreateOrUpdate DTOs
// Dùng cho Unit Type (Cần hứng thêm IsEntirePlace từ Form)
public class UnitTypeCreateDTO : BaseCreateOrUpdateAdminDTO
{
    public bool IsEntirePlace { get; set; } = false;
}

// Update
public class UnitTypeUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public bool IsEntirePlace { get; set; }
}

// Dùng cho Bed Type (Cần hứng thêm DefaultCapacity từ Form)
// Create
public class BedTypeCreateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(1, 10, ErrorMessage = "Sức chứa tối thiểu là 1")]
    public int DefaultCapacity { get; set; } = 1;

    public bool IsVaryingSize { get; set; }
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
}

// Update
public class BedTypeUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(1, 10)]
    public int DefaultCapacity { get; set; }

    public bool IsVaryingSize { get; set; }
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
}

// Dùng cho Room View (Không có field riêng, dùng lại base cũng được, hoặc tạo rỗng cho đồng bộ)
// Create
public class RoomViewCreateDTO : BaseCreateOrUpdateAdminDTO
{
}

// Update
public class RoomViewUpdateDTO : BaseCreateOrUpdateAdminDTO
{
}

public class RoomQualityCreateDTO : BaseCreateOrUpdateAdminDTO
{
    [Required(ErrorMessage = "Vui lòng chọn nhóm hạng phòng")]
    public int TypeId { get; set; }

    [Range(0, 100, ErrorMessage = "Thứ tự ưu tiên từ 0-100")]
    public int SortOrder { get; set; } = 0;
}

// 3. DTO Cập nhật (Sạch bóng TypeId)
public class RoomQualityUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(0, 100, ErrorMessage = "Thứ tự ưu tiên từ 0-100")]
    public int SortOrder { get; set; }
}