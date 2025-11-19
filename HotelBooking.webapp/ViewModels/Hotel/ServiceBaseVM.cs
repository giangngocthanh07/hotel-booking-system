using System.ComponentModel.DataAnnotations;
namespace HotelBooking.webapp.ViewModels.Hotel;

using System.Text.Json.Serialization;

public class ServiceTypeVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeleted { get; set; }

}

// Báo hiệu đây là lớp đa hình
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
// Khai báo các con và đặt tên định danh (discriminator) cho chúng
[JsonDerivedType(typeof(ServiceStandardVM), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportTransferVM), typeDiscriminator: "airportTransfer")]
public abstract class ServiceBaseVM
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; } = 0;
    public bool? IsDeleted { get; set; }
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