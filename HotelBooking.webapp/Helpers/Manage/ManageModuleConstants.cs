using HotelBooking.webapp.ViewModels.Admin;

namespace HotelBooking.webapp.Helpers;

/// <summary>
/// Constants and helper methods for Admin Management modules.
/// Replaces hardcoded strings in GlobalNavMenu.
/// </summary>
public static class ManageModuleConstants
{
    /// <summary>
    /// Module information record
    /// </summary>
    public record ModuleInfo(
        ManageModuleEnum Module,
        string DisplayName,
        string Route,
        string Icon,
        bool HasTypes // Has type dropdown?
    );

    /// <summary>
    /// Full list of all modules with details
    /// </summary>
    private static readonly List<ModuleInfo> _modules = new()
    {
        new(ManageModuleEnum.Service, "Service", "/admin/manage/service", "fa-solid fa-concierge-bell", true),
        new(ManageModuleEnum.Policy, "Policy", "/admin/manage/policy", "fa-solid fa-file-contract", true),
        new(ManageModuleEnum.Amenity, "Amenity", "/admin/manage/amenity", "fa-solid fa-wifi", true),
        new(ManageModuleEnum.RoomQuality, "Room Quality", "/admin/manage/roomquality", "fa-solid fa-star", true),
        new(ManageModuleEnum.UnitType, "Unit Type", "/admin/manage/unittype", "fa-solid fa-building", false),
        new(ManageModuleEnum.BedType, "Bed Type", "/admin/manage/bedtype", "fa-solid fa-bed", false),
        new(ManageModuleEnum.RoomView, "Room View", "/admin/manage/roomview", "fa-solid fa-mountain-sun", false),
    };

    /// <summary>
    /// Get all modules
    /// </summary>
    public static IReadOnlyList<ModuleInfo> GetAll() => _modules;

    /// <summary>
    /// Get module info by enum
    /// </summary>
    public static ModuleInfo? GetInfo(ManageModuleEnum module) 
        => _modules.FirstOrDefault(m => m.Module == module);

    /// <summary>
    /// Get module info by string name (case-insensitive)
    /// </summary>
    public static ModuleInfo? GetInfo(string moduleName)
    {
        if (Enum.TryParse<ManageModuleEnum>(moduleName, ignoreCase: true, out var module))
        {
            return GetInfo(module);
        }
        return null;
    }

    /// <summary>
    /// Get display name
    /// </summary>
    public static string GetDisplayName(ManageModuleEnum module) 
        => GetInfo(module)?.DisplayName ?? module.ToString();

    /// <summary>
    /// Get route URL
    /// </summary>
    public static string GetRoute(ManageModuleEnum module) 
        => GetInfo(module)?.Route ?? $"/admin/manage/{module.ToString().ToLower()}";

    /// <summary>
    /// Get icon class
    /// </summary>
    public static string GetIcon(ManageModuleEnum module) 
        => GetInfo(module)?.Icon ?? "fa-solid fa-cog";

    /// <summary>
    /// Check if module has type dropdown
    /// </summary>
    public static bool HasTypes(ManageModuleEnum module) 
        => GetInfo(module)?.HasTypes ?? false;
}
