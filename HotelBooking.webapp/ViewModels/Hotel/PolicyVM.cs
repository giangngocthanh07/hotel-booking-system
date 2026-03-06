using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelBooking.webapp.ViewModels.Admin.Base;

namespace HotelBooking.webapp.ViewModels.Admin;

// ===========================================================================
// ENUMS & TYPE DEFINITIONS
// ===========================================================================

public class PolicyTypeVM : BaseAdminVM { }

public enum PolicyTypeEnum
{
    CheckInOut = 1002,
    Cancellation = 1003,
    Children = 1004,
    Pets = 2002
}

// ===========================================================================
// POLYMORPHIC VM (Output - Display)
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
/// Policy regarding Check-In and Check-Out timings and fees.
/// </summary>
public class CheckInOutPolicyVM : PolicyVM
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Early check-in fee cannot be negative!")]
    public decimal? EarlyCheckInFee { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Late check-out fee cannot be negative!")]
    public decimal? LateCheckOutFee { get; set; }
}

/// <summary>
/// Policy regarding booking cancellations and refunds.
/// </summary>
public class CancellationPolicyVM : PolicyVM
{
    [Range(0, 365, ErrorMessage = "Days must be between 0 and 365!")]
    public int? DaysBeforeCheckIn { get; set; }

    [Range(0, 100, ErrorMessage = "Refund percentage must be between 0 and 100!")]
    public double? RefundPercent { get; set; }

    public bool IsRefundable { get; set; }
}

/// <summary>
/// Policy regarding children ages and extra bed requirements.
/// </summary>
public class ChildrenPolicyVM : PolicyVM
{
    [Range(0, 17, ErrorMessage = "Minimum age must be between 0 and 17!")]
    public int? MinAge { get; set; }

    [Range(0, 17, ErrorMessage = "Maximum age must be between 0 and 17!")]
    public int? MaxAge { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Extra bed fee cannot be negative!")]
    public decimal? ExtraBedFee { get; set; }
}

/// <summary>
/// Policy regarding pet stays and related fees.
/// </summary>
public class PetPolicyVM : PolicyVM
{
    [Range(0, double.MaxValue, ErrorMessage = "Pet fee cannot be negative!")]
    public decimal? PetFee { get; set; }

    public bool IsPetAllowed { get; set; }
}

// ===========================================================================
// POLYMORPHIC CREATE VM (Input - POST)
// ===========================================================================

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyCreateVM), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyCreateVM), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyCreateVM), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyCreateVM), typeDiscriminator: "pets")]
public abstract class PolicyCreateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Policy Type is required!")]
    public int TypeId { get; set; }
}

public class CheckInOutPolicyCreateVM : PolicyCreateVM
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Early check-in fee cannot be negative!")]
    public decimal? EarlyCheckInFee { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Late check-out fee cannot be negative!")]
    public decimal? LateCheckOutFee { get; set; }
}

public class CancellationPolicyCreateVM : PolicyCreateVM
{
    [Range(0, 365, ErrorMessage = "Days must be between 0 and 365!")]
    public int? DaysBeforeCheckIn { get; set; }

    [Range(0, 100, ErrorMessage = "Refund percentage must be between 0 and 100!")]
    public double? RefundPercent { get; set; }

    public bool IsRefundable { get; set; }
}

public class ChildrenPolicyCreateVM : PolicyCreateVM
{
    [Range(0, 17, ErrorMessage = "Minimum age must be between 0 and 17!")]
    public int? MinAge { get; set; }

    [Range(0, 17, ErrorMessage = "Maximum age must be between 0 and 17!")]
    public int? MaxAge { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Extra bed fee cannot be negative!")]
    public decimal? ExtraBedFee { get; set; }
}

public class PetPolicyCreateVM : PolicyCreateVM
{
    [Range(0, double.MaxValue, ErrorMessage = "Pet fee cannot be negative!")]
    public decimal? PetFee { get; set; }

    public bool IsPetAllowed { get; set; }
}

// ===========================================================================
// POLYMORPHIC UPDATE VM (Input - PUT)
// ===========================================================================

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyUpdateVM), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyUpdateVM), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyUpdateVM), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyUpdateVM), typeDiscriminator: "pets")]
public abstract class PolicyUpdateVM : BaseCreateOrUpdateAdminVM { }

public class CheckInOutPolicyUpdateVM : PolicyUpdateVM
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Early check-in fee cannot be negative!")]
    public decimal? EarlyCheckInFee { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Late check-out fee cannot be negative!")]
    public decimal? LateCheckOutFee { get; set; }
}

public class CancellationPolicyUpdateVM : PolicyUpdateVM
{
    [Range(0, 365, ErrorMessage = "Days must be between 0 and 365!")]
    public int? DaysBeforeCheckIn { get; set; }

    [Range(0, 100, ErrorMessage = "Refund percentage must be between 0 and 100!")]
    public double? RefundPercent { get; set; }

    public bool IsRefundable { get; set; }
}

public class ChildrenPolicyUpdateVM : PolicyUpdateVM
{
    [Range(0, 17, ErrorMessage = "Minimum age must be between 0 and 17!")]
    public int? MinAge { get; set; }

    [Range(0, 17, ErrorMessage = "Maximum age must be between 0 and 17!")]
    public int? MaxAge { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Extra bed fee cannot be negative!")]
    public decimal? ExtraBedFee { get; set; }
}

public class PetPolicyUpdateVM : PolicyUpdateVM
{
    [Range(0, double.MaxValue, ErrorMessage = "Pet fee cannot be negative!")]
    public decimal? PetFee { get; set; }

    public bool IsPetAllowed { get; set; }
}