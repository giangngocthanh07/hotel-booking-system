namespace HotelBooking.webapp.Helpers.Manage.Icon;

/// <summary>
/// Helper class to map Service names to FontAwesome icons.
/// Decouples logic from components for better maintainability and testing.
/// </summary>
public static class ServiceIconHelper
{
    private static readonly Dictionary<string, string> _iconMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Airport Transfer / Transportation Group
        ["airport"] = "fa-solid fa-plane-departure",
        ["sân bay"] = "fa-solid fa-plane-departure",
        ["shuttle"] = "fa-solid fa-van-shuttle",
        ["transfer"] = "fa-solid fa-car-side",
        ["đưa đón"] = "fa-solid fa-car-side",
        ["xe"] = "fa-solid fa-car",
        ["taxi"] = "fa-solid fa-taxi",

        // Dining / Food & Beverage Group
        ["breakfast"] = "fa-solid fa-bacon",
        ["bữa sáng"] = "fa-solid fa-bacon",
        ["lunch"] = "fa-solid fa-utensils",
        ["bữa trưa"] = "fa-solid fa-utensils",
        ["dinner"] = "fa-solid fa-utensils",
        ["bữa tối"] = "fa-solid fa-utensils",
        ["ăn"] = "fa-solid fa-utensils",
        ["food"] = "fa-solid fa-burger",
        ["đồ ăn"] = "fa-solid fa-burger",
        ["drink"] = "fa-solid fa-martini-glass",
        ["đồ uống"] = "fa-solid fa-martini-glass",
        ["minibar"] = "fa-solid fa-wine-bottle",
        ["coffee"] = "fa-solid fa-mug-hot",
        ["cà phê"] = "fa-solid fa-mug-hot",

        // Laundry Services Group
        ["laundry"] = "fa-solid fa-shirt",
        ["giặt"] = "fa-solid fa-shirt",
        ["ủi"] = "fa-solid fa-shirt",
        ["dry clean"] = "fa-solid fa-shirt",

        // Parking Group
        ["parking"] = "fa-solid fa-square-parking",
        ["đỗ xe"] = "fa-solid fa-square-parking",
        ["garage"] = "fa-solid fa-warehouse",

        // Spa / Massage / Wellness Group
        ["spa"] = "fa-solid fa-spa",
        ["massage"] = "fa-solid fa-hands",
        ["wellness"] = "fa-solid fa-heart-pulse",

        // Gym / Fitness Group
        ["gym"] = "fa-solid fa-dumbbell",
        ["fitness"] = "fa-solid fa-dumbbell",
        ["tập"] = "fa-solid fa-dumbbell",

        // Swimming Pool Group
        ["pool"] = "fa-solid fa-person-swimming",
        ["bể bơi"] = "fa-solid fa-person-swimming",
        ["hồ bơi"] = "fa-solid fa-person-swimming",

        // Tour / Excursion Group
        ["tour"] = "fa-solid fa-map-location-dot",
        ["excursion"] = "fa-solid fa-person-hiking",
        ["tham quan"] = "fa-solid fa-camera",

        // Meetings / Business Group
        ["meeting"] = "fa-solid fa-users",
        ["conference"] = "fa-solid fa-people-group",
        ["business"] = "fa-solid fa-briefcase",
        ["họp"] = "fa-solid fa-users",

        // Baby / Children Group
        ["baby"] = "fa-solid fa-baby",
        ["trẻ em"] = "fa-solid fa-child",
        ["childcare"] = "fa-solid fa-child-reaching",

        // Pet Services Group
        ["pet"] = "fa-solid fa-paw",
        ["thú cưng"] = "fa-solid fa-paw",

        // Room Service Group
        ["room service"] = "fa-solid fa-bell-concierge",
        ["phục vụ phòng"] = "fa-solid fa-bell-concierge",

        // Wifi / Internet Group
        ["wifi"] = "fa-solid fa-wifi",
        ["internet"] = "fa-solid fa-wifi",

        // Check-in / Check-out Group
        ["early"] = "fa-solid fa-clock",
        ["late"] = "fa-solid fa-clock",
        ["check-in"] = "fa-solid fa-right-to-bracket",
        ["check-out"] = "fa-solid fa-right-from-bracket",
    };

    /// <summary>
    /// Retrieves the FontAwesome icon class based on the Service name.
    /// </summary>
    public static string GetIcon(string? serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return "fa-solid fa-bell-concierge";

        var name = serviceName.ToLower();

        foreach (var mapping in _iconMappings)
        {
            if (name.Contains(mapping.Key))
                return mapping.Value;
        }

        return "fa-solid fa-box-open"; // Default icon
    }

    /// <summary>
    /// Retrieves the icon and its associated CSS color class based on the service type.
    /// </summary>
    public static (string Icon, string ColorClass) GetIconWithColor(string? serviceName, int? typeId = null)
    {
        var icon = GetIcon(serviceName);
        var color = GetColorByType(typeId);
        return (icon, color);
    }

    /// <summary>
    /// Determines the CSS color class based on the Service Type ID.
    /// </summary>
    private static string GetColorByType(int? typeId)
    {
        return typeId switch
        {
            1 => "text-primary",    // Standard
            2 => "text-info",       // Airport Transfer
            _ => "text-secondary"
        };
    }
}