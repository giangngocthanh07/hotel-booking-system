using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ServiceTypeDTO : BaseAdminDTO
{
}

public enum ServiceTypeEnum
{
    Standard = 1,
    AirportTransfer = 2,
}

// Báo hiệu đây là lớp đa hình
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
// Khai báo các con và đặt tên định danh (discriminator) cho chúng
[JsonDerivedType(typeof(ServiceStandardDTO), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportTransferDTO), typeDiscriminator: "airportTransfer")]
public abstract class ServiceBaseDTO : BaseAdminDTO
{
    public decimal Price { get; set; } = 0;
    public int TypeId { get; set; }
}

public class ServiceStandardDTO : ServiceBaseDTO
{
    // Thêm vào trường Additional bên DB
    [Required(ErrorMessage = "Vui lòng nhập đơn vị đo lường")]
    public string Unit { get; set; } = string.Empty;

}

public class ServiceAirportTransferDTO : ServiceBaseDTO
{

    public int? MaxPassengers { get; set; } = 0;

    public int? MaxLuggage { get; set; } = 0;

    public decimal? RoundTripPrice { get; set; }


    public decimal? AdditionalFee { get; set; }

    // 2 trường này chỉ có ý nghĩa khi AdditionalFee > 0
    // FE nên ẩn 2 trường này nếu AdditionalFee = 0
    public TimeSpan? AdditionalFeeStartTime { get; set; }
    public TimeSpan? AdditionalFeeEndTime { get; set; }

}

// DTO Thêm/Sửa dịch vụ
public abstract class ServiceCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public abstract int TargetTypeId { get; }
}

public class StdServiceCreateOrUpdateDTO : ServiceCreateOrUpdateDTO
{
    public override int TargetTypeId => (int)ServiceTypeEnum.Standard;
    [Required(ErrorMessage = "Vui lòng nhập đơn vị đo lường")]
    public string Unit { get; set; } = string.Empty;
}

public class AirportTransServiceCreateOrUpdateDTO : ServiceCreateOrUpdateDTO
{
    public override int TargetTypeId => (int)ServiceTypeEnum.AirportTransfer;
}

// DTO hiển thị dịch vụ đưa đón sân bay với các trường cụ thể
public class ServiceAdditionalDataAT
{
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
    public decimal? RoundTripPrice { get; set; }
    public decimal? AdditionalFee { get; set; }
    // Chúng ta đọc TimeSpan dưới dạng string trước
    public TimeSpan? AdditionalFeeStartTime { get; set; }
    public TimeSpan? AdditionalFeeEndTime { get; set; }
}

