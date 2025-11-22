using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ServiceTypeDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool? IsDeleted { get; set; }
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
public abstract class ServiceBaseDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; } = 0;
    public bool? IsDeleted { get; set; }
    public int ServiceTypeId { get; set; }
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

public class ManageServiceDTO
{
    // 1. List ServiceType để FE chọn loại dịch vụ
    public List<ServiceTypeDTO> ServiceTypes { get; set; } = new List<ServiceTypeDTO>();

    // Phần 2: Thông tin của Loại đang được chọn (để hiện tiêu đề, form add)
    public int SelectedTypeId { get; set; }
    public string? SelectedTypeName { get; set; }

    // Phần 3: Danh sách dịch vụ của loại đó (để fill Table)
    public List<ServiceBaseDTO> Services { get; set; } = new();
}