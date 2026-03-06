namespace HotelBooking.webapp.Helpers.Manage.Icon;

/// <summary>
/// Helper class to map Room View names to FontAwesome icons.
/// Decouples logic from components for better maintainability and testing.
/// </summary>
public static class RoomViewIconHelper
{
    private static readonly Dictionary<string, string> _iconMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Sea / Ocean Group
        ["sea"] = "fa-solid fa-water",
        ["ocean"] = "fa-solid fa-water",
        ["beach"] = "fa-solid fa-umbrella-beach",
        ["biển"] = "fa-solid fa-water",

        // Lake / River / Water Group
        ["lake"] = "fa-solid fa-water",
        ["river"] = "fa-solid fa-water",
        ["pool"] = "fa-solid fa-person-swimming",
        ["hồ"] = "fa-solid fa-water",
        ["sông"] = "fa-solid fa-water",
        ["bể bơi"] = "fa-solid fa-person-swimming",

        // Garden / Nature Group
        ["garden"] = "fa-solid fa-leaf",
        ["park"] = "fa-solid fa-tree",
        ["forest"] = "fa-solid fa-tree",
        ["vườn"] = "fa-solid fa-leaf",
        ["công viên"] = "fa-solid fa-tree",
        ["rừng"] = "fa-solid fa-tree",

        // Mountain / Hill Group
        ["mountain"] = "fa-solid fa-mountain",
        ["hill"] = "fa-solid fa-mountain-sun",
        ["núi"] = "fa-solid fa-mountain",
        ["đồi"] = "fa-solid fa-mountain-sun",

        // City / Urban Group
        ["city"] = "fa-solid fa-city",
        ["street"] = "fa-solid fa-road",
        ["urban"] = "fa-solid fa-building",
        ["skyline"] = "fa-solid fa-city",
        ["phố"] = "fa-solid fa-city",
        ["thành phố"] = "fa-solid fa-city",
        ["đường phố"] = "fa-solid fa-road",

        // Landmarks / Heritage Group
        ["landmark"] = "fa-solid fa-landmark",
        ["monument"] = "fa-solid fa-monument",
        ["temple"] = "fa-solid fa-place-of-worship",
        ["di tích"] = "fa-solid fa-landmark",
        ["đền"] = "fa-solid fa-place-of-worship",
        ["chùa"] = "fa-solid fa-place-of-worship",

        // Countryside / Rural Group
        ["countryside"] = "fa-solid fa-tractor",
        ["farm"] = "fa-solid fa-wheat-awn",
        ["field"] = "fa-solid fa-seedling",
        ["nông thôn"] = "fa-solid fa-tractor",
        ["đồng quê"] = "fa-solid fa-wheat-awn",
        ["ruộng"] = "fa-solid fa-seedling",

        // Sunset / Sunrise Group
        ["sunset"] = "fa-solid fa-sun",
        ["sunrise"] = "fa-solid fa-sun",
        ["hoàng hôn"] = "fa-solid fa-sun",
        ["bình minh"] = "fa-solid fa-sun",

        // Courtyard / Interior Group
        ["courtyard"] = "fa-solid fa-square",
        ["interior"] = "fa-solid fa-door-open",
        ["sân trong"] = "fa-solid fa-square",
        ["nội khu"] = "fa-solid fa-door-open",
    };

    /// <summary>
    /// Retrieves the FontAwesome icon class based on the Room View name.
    /// </summary>
    public static string GetIcon(string? viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            return "fa-solid fa-image";

        var name = viewName.ToLower();

        foreach (var mapping in _iconMappings)
        {
            if (name.Contains(mapping.Key))
                return mapping.Value;
        }

        return "fa-solid fa-street-view"; // Default icon
    }

    /// <summary>
    /// Retrieves the icon and its associated CSS color class based on the view type.
    /// </summary>
    public static (string Icon, string ColorClass) GetIconWithColor(string? viewName)
    {
        var icon = GetIcon(viewName);
        var color = GetColorByType(viewName);
        return (icon, color);
    }

    /// <summary>
    /// Determines the CSS color class based on keyword analysis of the view name.
    /// </summary>
    private static string GetColorByType(string? viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            return "text-secondary";

        var name = viewName.ToLower();

        // Water related: Blue/Cyan
        if (name.Contains("sea") || name.Contains("ocean") || name.Contains("lake") ||
            name.Contains("pool") || name.Contains("biển") || name.Contains("hồ"))
            return "text-info";

        // Nature related: Green
        if (name.Contains("garden") || name.Contains("forest") || name.Contains("park") ||
            name.Contains("vườn") || name.Contains("rừng"))
            return "text-success";

        // Landscape/Mountain: Grey
        if (name.Contains("mountain") || name.Contains("hill") || name.Contains("núi"))
            return "text-secondary";

        // Urban/City: Blue
        if (name.Contains("city") || name.Contains("phố") || name.Contains("street"))
            return "text-primary";

        // Sun related: Yellow/Orange
        if (name.Contains("sunset") || name.Contains("sunrise"))
            return "text-warning";

        return "text-secondary";
    }
}