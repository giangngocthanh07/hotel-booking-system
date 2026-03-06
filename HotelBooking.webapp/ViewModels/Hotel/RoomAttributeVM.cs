using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using HotelBooking.webapp.ViewModels.Admin.Base;

namespace HotelBooking.webapp.ViewModels.Admin;

// ===========================================================================
// ENUMS & REQUEST DTOs
// ===========================================================================

public enum RoomAttributeType
{
    UnitType = 1,
    BedType = 2,
    RoomView = 3,
    RoomQuality = 4
}

public class GetRoomAttributeRequest
{
    public RoomAttributeType Type { get; set; } = RoomAttributeType.UnitType;
    public PagingRequest Paging { get; set; } = new PagingRequest();
    public int? TypeId { get; set; } // Specifically used for RoomQuality filtering
}

// ===========================================================================
// POLYMORPHIC BASE (Output - Display)
// ===========================================================================

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(UnitTypeVM), typeDiscriminator: "unit")]
[JsonDerivedType(typeof(RoomViewVM), typeDiscriminator: "roomview")]
[JsonDerivedType(typeof(BedTypeVM), typeDiscriminator: "bed")]
[JsonDerivedType(typeof(RoomQualityVM), typeDiscriminator: "roomquality")]
public abstract class RoomAttributeVM : BaseAdminVM
{
    public abstract RoomAttributeType AttributeType { get; }
}

// --- 1. UNIT TYPE ---
public class UnitTypeVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.UnitType;
    public bool? IsEntirePlace { get; set; } = false;
}

public class UnitTypeCreateVM : BaseCreateOrUpdateAdminVM
{
    public bool? IsEntirePlace { get; set; } = false;
}

public class UnitTypeUpdateVM : BaseCreateOrUpdateAdminVM
{
    public bool? IsEntirePlace { get; set; }
}

// --- 2. ROOM VIEW ---
public class RoomViewVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomView;
}

public class RoomViewCreateVM : BaseCreateOrUpdateAdminVM { }

public class RoomViewUpdateVM : BaseCreateOrUpdateAdminVM { }

// --- 3. BED TYPE ---
public class BedTypeVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.BedType;

    [Required(ErrorMessage = "Default capacity is required!")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0!")]
    public int? DefaultCapacity { get; set; } = 1;

    [Range(0, 300, ErrorMessage = "Size cannot be negative or exceed 300 inches!")]
    public double MinWidth { get; set; }

    [Range(0, 300, ErrorMessage = "Size cannot be negative or exceed 300 inches!")]
    public double MaxWidth { get; set; }

    private bool? _isVaryingSize;
    public bool IsVaryingSize
    {
        get => _isVaryingSize ?? (MinWidth <= 0 && MaxWidth <= 0);
        set
        {
            _isVaryingSize = value;
            if (value)
            {
                MinWidth = 0;
                MaxWidth = 0;
            }
        }
    }

    public string SizeDisplay => IsVaryingSize ? "Varying" : $"{MinWidth}\" - {MaxWidth}\"";
}

public class BedTypeCreateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Default capacity is required!")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0!")]
    public int? DefaultCapacity { get; set; } = 1;

    [Range(0, 300, ErrorMessage = "Size cannot be negative or exceed 300 inches!")]
    public double MinWidth { get; set; }

    [Range(0, 300, ErrorMessage = "Size cannot be negative or exceed 300 inches!")]
    public double MaxWidth { get; set; }

    private bool? _isVaryingSize;
    public bool IsVaryingSize
    {
        get => _isVaryingSize ?? (MinWidth <= 0 && MaxWidth <= 0);
        set
        {
            _isVaryingSize = value;
            if (value)
            {
                MinWidth = 0;
                MaxWidth = 0;
            }
        }
    }
}

public class BedTypeUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Default capacity is required!")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0!")]
    public int? DefaultCapacity { get; set; } = 1;

    [Range(0, 300, ErrorMessage = "Size cannot be negative or exceed 300 inches!")]
    public double MinWidth { get; set; }

    [Range(0, 300, ErrorMessage = "Size cannot be negative or exceed 300 inches!")]
    public double MaxWidth { get; set; }

    private bool? _isVaryingSize;
    public bool IsVaryingSize
    {
        get => _isVaryingSize ?? (MinWidth <= 0 && MaxWidth <= 0);
        set
        {
            _isVaryingSize = value;
            if (value)
            {
                MinWidth = 0;
                MaxWidth = 0;
            }
        }
    }
}

// --- 4. ROOM QUALITY ---
public class RoomQualityVM : RoomAttributeVM
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomQuality;

    [Range(0, 10, ErrorMessage = "Sort order must be between 0 and 10")]
    public int? SortOrder { get; set; } = 0;

    [Required(ErrorMessage = "Room Quality Type is required!")]
    public int TypeId { get; set; }
}

public class RoomQualityCreateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Please select a room category group!")]
    public int TypeId { get; set; }

    [Range(0, 10, ErrorMessage = "Sort order must be between 0 and 10")]
    public int SortOrder { get; set; } = 0;
}

public class RoomQualityUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Range(0, 10, ErrorMessage = "Sort order must be between 0 and 10")]
    public int? SortOrder { get; set; } = 0;
}