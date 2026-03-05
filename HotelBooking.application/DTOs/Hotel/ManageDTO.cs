using System.Text.Json.Serialization;

public enum ManageModuleEnum
{
    Service = 1,
    Policy = 2,
    Amenity = 3,
    RoomQuality = 4,
    UnitType = 5,
    BedType = 6,
    RoomView = 7

    // Add Room = 3, Staff = 4... later
}


// DTO used for Sidebar list
public class ManageTypeDTO // Or reuse BaseAdminDTO if you want to edit directly
{
    public int? Id { get; set; } // Null if it's a flat module
    public string Name { get; set; } = string.Empty;
}

// DTO to capture input parameters for the Get Menu API
public class ManageMenuRequest
{
    // This is the main parameter we want to validate
    public ManageModuleEnum Module { get; set; }
}

// Result for API: /get-types/{module}
public class ManageMenuResult
{
    // List of types (e.g., Standard, VIP...)
    public List<ManageTypeDTO> Types { get; set; } = new();

    // (Optional) Suggest default ID if you want BE to decide the default logic
    // If you let FE select the first one by itself, this line is not needed.
    public int? DefaultSelectedId { get; set; }
}

// T is the data type of the item (e.g., ServiceBaseDTO, PolicyDTO...)
public class PagedManageResult<T> : PagedResult<T>
{
    public int? SelectedTypeId { get; set; }

    public PagedManageResult(List<T> items, int count, int pageIndex, int pageSize, int? selectedType)
        : base(items, count, pageIndex, pageSize)
    {
        SelectedTypeId = selectedType;
    }
}
