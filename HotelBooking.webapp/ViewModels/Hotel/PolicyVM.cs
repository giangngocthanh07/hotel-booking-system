using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

// ===========================================================================
// ENUMS & TYPE DEFINITIONS
// ===========================================================================
public class PolicyTypeVM : BaseAdminVM
{
}

public enum PolicyTypeEnum
{
    CheckInOut = 1002,
    Cancellation = 1003,
    Children = 1004,
    Pets = 2002
}

// ===========================================================================
// POLYMORPHIC VM (Output - Hiển thị)
// ===========================================================================
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyVM), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyVM), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyVM), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyVM), typeDiscriminator: "pets")]
public abstract class PolicyVM : BaseAdminVM
{
    public int TypeId { get; set; }
}

/// <summary>
/// Check-In/Check-Out Policy (TypeId: 1002)
/// </summary>
public class CheckInOutPolicyVM : PolicyVM
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí check-in sớm không được âm!")]
    public decimal? EarlyCheckInFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí check-out muộn không được âm!")]
    public decimal? LateCheckOutFee { get; set; }
}

/// <summary>
/// Cancellation Policy (TypeId: 1003)
/// </summary>
public class CancellationPolicyVM : PolicyVM
{
    [Range(0, 365, ErrorMessage = "Số ngày phải từ 0 đến 365!")]
    public int? DaysBeforeCheckIn { get; set; }
    
    [Range(0, 100, ErrorMessage = "Phần trăm hoàn tiền phải từ 0 đến 100!")]
    public double? RefundPercent { get; set; }
    
    public bool IsRefundable { get; set; }
}

/// <summary>
/// Children & Extra Bed Policy (TypeId: 1004)
/// </summary>
public class ChildrenPolicyVM : PolicyVM
{
    [Range(0, 17, ErrorMessage = "Tuổi tối thiểu phải từ 0 đến 17!")]
    public int? MinAge { get; set; }
    
    [Range(0, 17, ErrorMessage = "Tuổi tối đa phải từ 0 đến 17!")]
    public int? MaxAge { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí giường phụ không được âm!")]
    public decimal? ExtraBedFee { get; set; }
}

/// <summary>
/// Pet Policy (TypeId: 2002)
/// </summary>
public class PetPolicyVM : PolicyVM
{
    [Range(0, double.MaxValue, ErrorMessage = "Phí thú cưng không được âm!")]
    public decimal? PetFee { get; set; }
    
    public bool IsPetAllowed { get; set; }
}

// ===========================================================================
// POLYMORPHIC CREATE VM (Input - Tạo mới)
// ===========================================================================
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyCreateVM), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyCreateVM), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyCreateVM), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyCreateVM), typeDiscriminator: "pets")]
public abstract class PolicyCreateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Loại chính sách không được để trống!")]
    public int TypeId { get; set; }
}

public class CheckInOutPolicyCreateVM : PolicyCreateVM
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí check-in sớm không được âm!")]
    public decimal? EarlyCheckInFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí check-out muộn không được âm!")]
    public decimal? LateCheckOutFee { get; set; }
}

public class CancellationPolicyCreateVM : PolicyCreateVM
{
    [Range(0, 365, ErrorMessage = "Số ngày phải từ 0 đến 365!")]
    public int? DaysBeforeCheckIn { get; set; }
    
    [Range(0, 100, ErrorMessage = "Phần trăm hoàn tiền phải từ 0 đến 100!")]
    public double? RefundPercent { get; set; }
    
    public bool IsRefundable { get; set; }
}

public class ChildrenPolicyCreateVM : PolicyCreateVM
{
    [Range(0, 17, ErrorMessage = "Tuổi tối thiểu phải từ 0 đến 17!")]
    public int? MinAge { get; set; }
    
    [Range(0, 17, ErrorMessage = "Tuổi tối đa phải từ 0 đến 17!")]
    public int? MaxAge { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí giường phụ không được âm!")]
    public decimal? ExtraBedFee { get; set; }
}

public class PetPolicyCreateVM : PolicyCreateVM
{
    [Range(0, double.MaxValue, ErrorMessage = "Phí thú cưng không được âm!")]
    public decimal? PetFee { get; set; }
    
    public bool IsPetAllowed { get; set; }
}

// ===========================================================================
// POLYMORPHIC UPDATE VM (Input - Cập nhật)
// ===========================================================================
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyUpdateVM), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyUpdateVM), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyUpdateVM), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyUpdateVM), typeDiscriminator: "pets")]
public abstract class PolicyUpdateVM : BaseCreateOrUpdateAdminVM
{
}

public class CheckInOutPolicyUpdateVM : PolicyUpdateVM
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí check-in sớm không được âm!")]
    public decimal? EarlyCheckInFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí check-out muộn không được âm!")]
    public decimal? LateCheckOutFee { get; set; }
}

public class CancellationPolicyUpdateVM : PolicyUpdateVM
{
    [Range(0, 365, ErrorMessage = "Số ngày phải từ 0 đến 365!")]
    public int? DaysBeforeCheckIn { get; set; }
    
    [Range(0, 100, ErrorMessage = "Phần trăm hoàn tiền phải từ 0 đến 100!")]
    public double? RefundPercent { get; set; }
    
    public bool IsRefundable { get; set; }
}

public class ChildrenPolicyUpdateVM : PolicyUpdateVM
{
    [Range(0, 17, ErrorMessage = "Tuổi tối thiểu phải từ 0 đến 17!")]
    public int? MinAge { get; set; }
    
    [Range(0, 17, ErrorMessage = "Tuổi tối đa phải từ 0 đến 17!")]
    public int? MaxAge { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Phí giường phụ không được âm!")]
    public decimal? ExtraBedFee { get; set; }
}

public class PetPolicyUpdateVM : PolicyUpdateVM
{
    [Range(0, double.MaxValue, ErrorMessage = "Phí thú cưng không được âm!")]
    public decimal? PetFee { get; set; }
    
    public bool IsPetAllowed { get; set; }
}