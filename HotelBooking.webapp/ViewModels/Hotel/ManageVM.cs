namespace HotelBooking.webapp.ViewModels.Admin;

/// <summary>
/// Defines the various management modules available in the Admin Panel.
/// </summary>
public enum ManageModuleEnum
{
    Service = 1,
    Policy = 2,
    Amenity = 3,
    RoomQuality = 4,
    UnitType = 5,
    BedType = 6,
    RoomView = 7

    // Future extensions: Room = 8, Staff = 9, etc.
}

/// <summary>
/// Represents an entry in the management sidebar or dropdown.
/// Used to categorize data within a module (e.g., Standard, VIP, etc.).
/// </summary>
public class ManageTypeVM
{
    /// <summary>
    /// The unique identifier for the type. Null if the module is flat (no sub-types).
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Display name for the category/type.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data structure for the Sidebar menu result.
/// </summary>
public class ManageMenuResultVM
{
    /// <summary>
    /// List of available sub-types or categories within the selected module.
    /// </summary>
    public List<ManageTypeVM> Types { get; set; } = new();

    /// <summary>
    /// Suggested ID to be selected by default (provided by Backend).
    /// </summary>
    public int? DefaultSelectedId { get; set; }
}

/// <summary>
/// Generic wrapper for paginated management data, including context about the selected type.
/// </summary>
/// <typeparam name="T">The specific View Model type (e.g., ServiceVM, PolicyVM).</typeparam>
public class PagedManageResult<T> : PagedResult<T>
{
    /// <summary>
    /// The current type ID filter being applied to the result set.
    /// </summary>
    public int? SelectedTypeId { get; set; }
}