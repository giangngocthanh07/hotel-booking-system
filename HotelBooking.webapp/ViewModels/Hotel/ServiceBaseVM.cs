using System.ComponentModel.DataAnnotations;
namespace HotelBooking.webapp.ViewModels.Hotel;

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
public abstract class ServiceBaseVM : BaseAdminVM
{
    public decimal Price { get; set; } = 0;
    public int ServiceTypeId { get; set; }
}

public class ServiceStandardVM : ServiceBaseVM
{
    // Các trường đặc thù cho Standard Service có thể được thêm vào đây
    [Required(ErrorMessage = "Unit is required.")]
    public string Unit { get; set; } = string.Empty;
}

public class ServiceAirportTransferVM : ServiceBaseVM
{
    // Các trường đặc thù cho Airport Transfer Service có thể được thêm vào đây
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
    public decimal? RoundTripPrice { get; set; }
    public decimal? AdditionalFee { get; set; }

    // 2 trường này chỉ có ý nghĩa khi AdditionalFee > 0
    // FE nên ẩn 2 trường này nếu AdditionalFee = 0
    public TimeSpan? AdditionalFeeStartTime { get; set; }
    public TimeSpan? AdditionalFeeEndTime { get; set; }
}

// DTO Thêm/Sửa dịch vụ
public abstract class ServiceCreateOrUpdateVM : BaseCreateOrUpdateAdminVM
{
    public abstract int TargetTypeId { get; }
}

public class StdServiceCreateOrUpdateVM : ServiceCreateOrUpdateVM
{
    public override int TargetTypeId => (int)ServiceTypeEnum.Standard;
    [Required(ErrorMessage = "Vui lòng nhập đơn vị đo lường")]
    public string Unit { get; set; } = string.Empty;
}

public class AirportTransServiceCreateOrUpdateVM : ServiceCreateOrUpdateVM
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

public class ManageServiceVM
{
    public List<ServiceTypeVM> ServiceTypes { get; set; } = new();
    public List<ServiceBaseVM> Services { get; set; } = new();
    public int SelectedTypeId { get; set; }
    public string? SelectedTypeName { get; set; }
}