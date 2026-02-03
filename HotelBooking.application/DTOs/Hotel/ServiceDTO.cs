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
public abstract class ServiceDTO : BaseAdminDTO
{
    [Range(1000, double.MaxValue, ErrorMessage = "Dịch vụ phải có giá từ 1,000đ trở lên!")]
    public decimal Price { get; set; } = 0;
    [Required(ErrorMessage = "Loại dịch vụ không được để trống!")]
    public int TypeId { get; set; }
}

public class ServiceStandardDTO : ServiceDTO
{
    // Thêm vào trường Additional bên DB
    [Required(ErrorMessage = "Đơn vị tính không được để trống!")]
    [MaxLength(20, ErrorMessage = "Đơn vị tính quá dài (tối đa 20 ký tự)!")]
    public string Unit { get; set; } = string.Empty;

}

public class ServiceAirportTransferDTO : ServiceDTO
{

    // Câu hỏi: Có tính phí 1 chiều không?
    public bool IsOneWayPaid { get; set; }

    // Câu hỏi: Có hỗ trợ 2 chiều không?
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    // Câu hỏi: Có phụ phí đêm không?
    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    // Thông số cơ bản
    [Range(1, 45, ErrorMessage = "Số hành khách phải từ 1 đến 45!")]
    public int? MaxPassengers { get; set; }
    [Range(1, 45, ErrorMessage = "Số hành lý phải từ 1 đến 45!")]
    public int? MaxLuggage { get; set; }

}

// DTO hiển thị dịch vụ đưa đón sân bay với các trường cụ thể
public class ServiceAirportAdditionalData
{
    // Câu hỏi: Có tính phí 1 chiều không?
    public bool IsOneWayPaid { get; set; }

    // Câu hỏi: Có hỗ trợ 2 chiều không?
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    // Câu hỏi: Có phụ phí đêm không?
    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public string? AdditionalFeeStartTime { get; set; }
    public string? AdditionalFeeEndTime { get; set; }

    // Thông số cơ bản
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardCreateDTO), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportCreateDTO), typeDiscriminator: "airport")]
// DTO Thêm/Sửa dịch vụ
public abstract class ServiceCreateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(1000, double.MaxValue, ErrorMessage = "Dịch vụ phải có giá từ 1,000đ trở lên!")]
    public decimal Price { get; set; } = 0;

    // Getter ảo để backend biết đang tạo loại gì
    [JsonIgnore]
    public abstract int TargetTypeId { get; }
}

public class ServiceStandardCreateDTO : ServiceCreateDTO
{
    public override int TargetTypeId => (int)ServiceTypeEnum.Standard;
    [Required(ErrorMessage = "Vui lòng nhập đơn vị đo lường")]
    [MaxLength(20, ErrorMessage = "Đơn vị tính quá dài (tối đa 20 ký tự)!")]
    public string Unit { get; set; } = string.Empty;
}

public class ServiceAirportCreateDTO : ServiceCreateDTO
{
    public override int TargetTypeId => (int)ServiceTypeEnum.AirportTransfer;

    // Câu hỏi: Có tính phí 1 chiều không?
    public bool IsOneWayPaid { get; set; }

    // Câu hỏi: Có hỗ trợ 2 chiều không?
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    // Câu hỏi: Có phụ phí đêm không?
    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    // Thông số cơ bản
    [Range(1, 45, ErrorMessage = "Số hành khách phải từ 1 đến 45!")]
    public int? MaxPassengers { get; set; }
    [Range(1, 45, ErrorMessage = "Số hành lý phải từ 1 đến 45!")]
    public int? MaxLuggage { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardUpdateDTO), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportUpdateDTO), typeDiscriminator: "airport")]
public abstract class ServiceUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(1000, double.MaxValue, ErrorMessage = "Dịch vụ phải có giá từ 1,000đ trở lên!")]
    public decimal Price { get; set; }

    // Không có TargetTypeId -> Không thể đổi loại dịch vụ
}

// 1. Update Standard
public class ServiceStandardUpdateDTO : ServiceUpdateDTO
{
    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
}

// 2. Update Airport
public class ServiceAirportUpdateDTO : ServiceUpdateDTO
{
    public bool IsOneWayPaid { get; set; }
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    [Range(1, 45, ErrorMessage = "Số hành khách phải từ 1 đến 45!")]
    public int? MaxPassengers { get; set; }
    [Range(1, 45, ErrorMessage = "Số hành lý phải từ 1 đến 45!")]
    public int? MaxLuggage { get; set; }
}



