using System.Text.Json.Serialization;

// ===========================================================================
// ENUMS & TYPE DEFINITIONS
// ===========================================================================
public class PolicyTypeDTO : BaseAdminDTO
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
// POLYMORPHIC DTO (Output - Hiển thị)
// ===========================================================================
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyDTO), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyDTO), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyDTO), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyDTO), typeDiscriminator: "pets")]
public abstract class PolicyDTO : BaseAdminDTO
{
    public int TypeId { get; set; }
}

/// <summary>
/// Check-In/Check-Out Policy (TypeId: 1002)
/// </summary>
public class CheckInOutPolicyDTO : PolicyDTO
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal? EarlyCheckInFee { get; set; }
    public decimal? LateCheckOutFee { get; set; }
}

/// <summary>
/// Cancellation Policy (TypeId: 1003)
/// </summary>
public class CancellationPolicyDTO : PolicyDTO
{
    public int? DaysBeforeCheckIn { get; set; }
    public double? RefundPercent { get; set; }
    public bool IsRefundable { get; set; }
}

/// <summary>
/// Children & Extra Bed Policy (TypeId: 1004)
/// </summary>
public class ChildrenPolicyDTO : PolicyDTO
{
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? ExtraBedFee { get; set; }
}

/// <summary>
/// Pet Policy (TypeId: 2002)
/// </summary>
public class PetPolicyDTO : PolicyDTO
{
    public decimal? PetFee { get; set; }
    public bool IsPetAllowed { get; set; }
}

// ===========================================================================
// ADDITIONAL DATA CLASSES (Dùng cho JSON serialization)
// ===========================================================================
public class CheckInOutAdditionalData
{
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public decimal? EarlyCheckInFee { get; set; }
    public decimal? LateCheckOutFee { get; set; }
}

public class CancellationAdditionalData
{
    public int? DaysBeforeCheckIn { get; set; }
    public double? RefundPercent { get; set; }
    public bool IsRefundable { get; set; }
}

public class ChildrenAdditionalData
{
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? ExtraBedFee { get; set; }
}

public class PetAdditionalData
{
    public decimal? PetFee { get; set; }
    public bool IsPetAllowed { get; set; }
}

// ===========================================================================
// POLYMORPHIC CREATE DTO (Input - Tạo mới)
// ===========================================================================
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyCreateDTO), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyCreateDTO), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyCreateDTO), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyCreateDTO), typeDiscriminator: "pets")]
public abstract class PolicyCreateDTO : BaseCreateOrUpdateAdminDTO
{
    public int TypeId { get; set; }
}

public class CheckInOutPolicyCreateDTO : PolicyCreateDTO
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal? EarlyCheckInFee { get; set; }
    public decimal? LateCheckOutFee { get; set; }
}

public class CancellationPolicyCreateDTO : PolicyCreateDTO
{
    public int? DaysBeforeCheckIn { get; set; }
    public double? RefundPercent { get; set; }
    public bool IsRefundable { get; set; }
}

public class ChildrenPolicyCreateDTO : PolicyCreateDTO
{
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? ExtraBedFee { get; set; }
}

public class PetPolicyCreateDTO : PolicyCreateDTO
{
    public decimal? PetFee { get; set; }
    public bool IsPetAllowed { get; set; }
}

// ===========================================================================
// POLYMORPHIC UPDATE DTO (Input - Cập nhật)
// ===========================================================================
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(CheckInOutPolicyUpdateDTO), typeDiscriminator: "checkInOut")]
[JsonDerivedType(typeof(CancellationPolicyUpdateDTO), typeDiscriminator: "cancellation")]
[JsonDerivedType(typeof(ChildrenPolicyUpdateDTO), typeDiscriminator: "children")]
[JsonDerivedType(typeof(PetPolicyUpdateDTO), typeDiscriminator: "pets")]
public abstract class PolicyUpdateDTO : BaseCreateOrUpdateAdminDTO
{
}

public class CheckInOutPolicyUpdateDTO : PolicyUpdateDTO
{
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal? EarlyCheckInFee { get; set; }
    public decimal? LateCheckOutFee { get; set; }
}

public class CancellationPolicyUpdateDTO : PolicyUpdateDTO
{
    public int? DaysBeforeCheckIn { get; set; }
    public double? RefundPercent { get; set; }
    public bool IsRefundable { get; set; }
}

public class ChildrenPolicyUpdateDTO : PolicyUpdateDTO
{
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? ExtraBedFee { get; set; }
}

public class PetPolicyUpdateDTO : PolicyUpdateDTO
{
    public decimal? PetFee { get; set; }
    public bool IsPetAllowed { get; set; }
}
