using System.Text.Json.Serialization;

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
public class UnitTypeCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public bool? IsEntirePlace { get; set; } = false;
}

// Dùng cho Bed Type (Cần hứng thêm DefaultCapacity từ Form)
public class BedTypeCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public int DefaultCapacity { get; set; } = 1;
}

// Dùng cho Room View (Không có field riêng, dùng lại base cũng được, hoặc tạo rỗng cho đồng bộ)
public class RoomViewCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
}

public class RoomQualityCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public int? SortOrder { get; set; } = 0;
    public int TypeId { get; set; }
}