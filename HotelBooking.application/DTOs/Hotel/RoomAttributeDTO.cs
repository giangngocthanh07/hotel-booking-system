using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class GetRoomAttributeRequest
{
    public RoomAttributeType Type { get; set; }
    public PagingRequest Paging { get; set; } = new(); // Init to avoid null
    public int? TypeId { get; set; }
}

public enum RoomAttributeType
{
    UnitType = 1,
    BedType = 2,
    RoomView = 3,
    RoomQuality = 4
}

// Indicate this is a polymorphic class
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
// Declare children and set identifiers (discriminators) for them
[JsonDerivedType(typeof(UnitTypeDTO), typeDiscriminator: "unit")]
[JsonDerivedType(typeof(RoomViewDTO), typeDiscriminator: "roomview")]
[JsonDerivedType(typeof(BedTypeDTO), typeDiscriminator: "bed")]
[JsonDerivedType(typeof(RoomQualityDTO), typeDiscriminator: "roomquality")]
public abstract class RoomAttributeDTO : BaseAdminDTO
{
    public abstract RoomAttributeType AttributeType { get; }
}

public class UnitTypeDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.UnitType; // Auto assign 1
    public bool? IsEntirePlace { get; set; } = false;
}

public class RoomViewDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomView; // Auto assign 3
}

public class BedTypeDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.BedType; // Auto assign 2
    public int? DefaultCapacity { get; set; } = 1;

    // --- ADDITIONAL TECHNICAL PARAMETER FIELDS ---

    // Minimum width (Inch)
    public double MinWidth { get; set; }

    // Maximum width (Inch)
    public double MaxWidth { get; set; }

    // Logic flag to display "Varying Size" Badge or number range on UI
    public bool IsVaryingSize => MinWidth <= 0 && MaxWidth <= 0;

    // Helper property to display text nicely (e.g., "39 - 75 inch" or "Varying size")
    public string SizeDisplay => IsVaryingSize
        ? "Varying size"
        : $"{MinWidth}\" - {MaxWidth}\"";
}

public class BedTypeAdditionalData
{
    // Width (Inch)
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }

    // Flag if size is not fixed
    public bool IsVaryingSize => MinWidth <= 0 && MaxWidth <= 0;
}

public class RoomQualityGroupDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomQuality; // Auto assign 4
    public int? SortOrder { get; set; } = 0;

}

public class RoomQualityDTO : RoomAttributeDTO
{
    public override RoomAttributeType AttributeType => RoomAttributeType.RoomQuality; // Auto assign 4
    public int? SortOrder { get; set; } = 0;
    public int TypeId { get; set; }
}

// CreateOrUpdate DTOs
// Used for Unit Type (Needs to catch IsEntirePlace from Form)
public class UnitTypeCreateDTO : BaseCreateOrUpdateAdminDTO
{
    public bool IsEntirePlace { get; set; } = false;
}

// Update
public class UnitTypeUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public bool IsEntirePlace { get; set; }
}

// Used for Bed Type (Needs to catch DefaultCapacity from Form)
// Create
public class BedTypeCreateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(1, 10, ErrorMessage = "Minimum capacity is 1")]
    public int DefaultCapacity { get; set; } = 1;

    public bool IsVaryingSize { get; set; }
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
}

// Update
public class BedTypeUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(1, 10)]
    public int DefaultCapacity { get; set; }

    public bool IsVaryingSize { get; set; }
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
}

// Used for Room View (No separate field, reusing base is fine, or create empty for consistency)
// Create
public class RoomViewCreateDTO : BaseCreateOrUpdateAdminDTO
{
}

// Update
public class RoomViewUpdateDTO : BaseCreateOrUpdateAdminDTO
{
}

public class RoomQualityCreateDTO : BaseCreateOrUpdateAdminDTO
{
    [Required(ErrorMessage = "Please select a room quality group")]
    public int TypeId { get; set; }

    [Range(0, 100, ErrorMessage = "Priority order from 0-100")]
    public int SortOrder { get; set; } = 0;
}

// 3. Update DTO (Clean of TypeId)
public class RoomQualityUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    [Range(0, 100, ErrorMessage = "Priority order from 0-100")]
    public int SortOrder { get; set; }
}