using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ServiceTypeVM : BaseAdminVM
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
[JsonDerivedType(typeof(ServiceStandardVM), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportTransferVM), typeDiscriminator: "airportTransfer")]
public abstract class ServiceVM : BaseAdminVM
{
    [Range(1000, double.MaxValue, ErrorMessage = "Dịch vụ phải có giá từ 1,000đ trở lên!")]
    public decimal Price { get; set; } = 0;

    [Required(ErrorMessage = "Loại dịch vụ không được để trống!")]
    public int TypeId { get; set; }
}

public class ServiceStandardVM : ServiceVM
{
    [Required(ErrorMessage = "Đơn vị tính không được để trống!")]
    [MaxLength(20, ErrorMessage = "Đơn vị tính quá dài (tối đa 20 ký tự)!")]
    public string Unit { get; set; } = string.Empty;
}

public class ServiceAirportTransferVM : ServiceVM
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

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardCreateVM), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportCreateVM), typeDiscriminator: "airport")]
// DTO Thêm/Sửa dịch vụ
public abstract class ServiceCreateVM : BaseCreateOrUpdateAdminVM
{
    [Range(1000, double.MaxValue, ErrorMessage = "Dịch vụ phải có giá từ 1,000đ trở lên!")]
    public decimal Price { get; set; } = 0;

    // Property trừu tượng để UI binding (VD: Standard = 1, Airport = 2)
    [JsonIgnore]
    public abstract int TargetTypeId { get; }
}

// 3.1. Create Standard
public class ServiceStandardCreateVM : ServiceCreateVM
{
    public override int TargetTypeId => (int)ServiceTypeEnum.Standard;

    [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
    [MaxLength(20, ErrorMessage = "Đơn vị tính tối đa 20 ký tự")]
    public string Unit { get; set; } = string.Empty;
}

// 3.2. Create Airport
public class ServiceAirportCreateVM : ServiceCreateVM
{
    public override int TargetTypeId => (int)ServiceTypeEnum.AirportTransfer;

    // Logic vé
    public bool IsOneWayPaid { get; set; }

    // Logic khứ hồi
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá khứ hồi không hợp lệ")]
    public decimal? RoundTripPrice { get; set; }

    // Logic phụ phí đêm
    public bool HasNightFee { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    // Thông số
    [Range(1, 45, ErrorMessage = "Số khách phải từ 1-45")]
    public int? MaxPassengers { get; set; }

    [Range(1, 45, ErrorMessage = "Số hành lý phải từ 1-45")]
    public int? MaxLuggage { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardUpdateVM), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportUpdateVM), typeDiscriminator: "airport")]
public abstract class ServiceUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Range(1000, double.MaxValue, ErrorMessage = "Dịch vụ phải có giá từ 1,000đ trở lên!")]
    public decimal Price { get; set; } = 0;
}

// 4.1. Update Standard
public class ServiceStandardUpdateVM : ServiceUpdateVM
{
    [Required(ErrorMessage = "Vui lòng nhập đơn vị tính")]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
}

// 4.2. Update Airport
public class ServiceAirportUpdateVM : ServiceUpdateVM
{
    public bool IsOneWayPaid { get; set; }

    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? RoundTripPrice { get; set; }

    public bool HasNightFee { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? AdditionalFee { get; set; }

    
    [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu")]
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập giờ kết thúc")]
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    [Range(1, 45, ErrorMessage = "Số khách phải từ 1-45")]
    public int? MaxPassengers { get; set; }

    [Range(1, 45, ErrorMessage = "Số hành lý phải từ 1-45")]
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
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    // Thông số cơ bản
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
}

public class ManageServiceVM
{
    public List<ServiceTypeVM> ServiceTypes { get; set; } = new();
    public List<ServiceVM> Services { get; set; } = new();
    public int SelectedTypeId { get; set; }
    public string? SelectedTypeName { get; set; }
}