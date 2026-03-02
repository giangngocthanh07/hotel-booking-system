namespace HotelBooking.webapp.Helpers;

/// <summary>
/// Helper class để map tên Service sang FontAwesome icon.
/// Tách logic ra khỏi component để dễ maintain và test.
/// </summary>
public static class ServiceIconHelper
{
    private static readonly Dictionary<string, string> _iconMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Đưa đón sân bay / Xe
        ["airport"] = "fa-solid fa-plane-departure",
        ["sân bay"] = "fa-solid fa-plane-departure",
        ["shuttle"] = "fa-solid fa-van-shuttle",
        ["transfer"] = "fa-solid fa-car-side",
        ["đưa đón"] = "fa-solid fa-car-side",
        ["xe"] = "fa-solid fa-car",
        ["taxi"] = "fa-solid fa-taxi",
        
        // Ăn uống
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
        
        // Giặt ủi
        ["laundry"] = "fa-solid fa-shirt",
        ["giặt"] = "fa-solid fa-shirt",
        ["ủi"] = "fa-solid fa-shirt",
        ["dry clean"] = "fa-solid fa-shirt",
        
        // Đỗ xe
        ["parking"] = "fa-solid fa-square-parking",
        ["đỗ xe"] = "fa-solid fa-square-parking",
        ["garage"] = "fa-solid fa-warehouse",
        
        // Spa / Massage
        ["spa"] = "fa-solid fa-spa",
        ["massage"] = "fa-solid fa-hands",
        ["wellness"] = "fa-solid fa-heart-pulse",
        
        // Gym / Fitness
        ["gym"] = "fa-solid fa-dumbbell",
        ["fitness"] = "fa-solid fa-dumbbell",
        ["tập"] = "fa-solid fa-dumbbell",
        
        // Hồ bơi
        ["pool"] = "fa-solid fa-person-swimming",
        ["bể bơi"] = "fa-solid fa-person-swimming",
        ["hồ bơi"] = "fa-solid fa-person-swimming",
        
        // Tour / Excursion
        ["tour"] = "fa-solid fa-map-location-dot",
        ["excursion"] = "fa-solid fa-person-hiking",
        ["tham quan"] = "fa-solid fa-camera",
        
        // Phòng họp / Business
        ["meeting"] = "fa-solid fa-users",
        ["conference"] = "fa-solid fa-people-group",
        ["business"] = "fa-solid fa-briefcase",
        ["họp"] = "fa-solid fa-users",
        
        // Baby / Trẻ em
        ["baby"] = "fa-solid fa-baby",
        ["trẻ em"] = "fa-solid fa-child",
        ["childcare"] = "fa-solid fa-child-reaching",
        
        // Pet / Thú cưng
        ["pet"] = "fa-solid fa-paw",
        ["thú cưng"] = "fa-solid fa-paw",
        
        // Room service
        ["room service"] = "fa-solid fa-bell-concierge",
        ["phục vụ phòng"] = "fa-solid fa-bell-concierge",
        
        // Wifi / Internet
        ["wifi"] = "fa-solid fa-wifi",
        ["internet"] = "fa-solid fa-wifi",
        
        // Early check-in / Late check-out
        ["early"] = "fa-solid fa-clock",
        ["late"] = "fa-solid fa-clock",
        ["check-in"] = "fa-solid fa-right-to-bracket",
        ["check-out"] = "fa-solid fa-right-from-bracket",
    };

    /// <summary>
    /// Lấy FontAwesome icon class dựa trên tên Service
    /// </summary>
    public static string GetIcon(string? serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return "fa-solid fa-concierge-bell";

        var name = serviceName.ToLower();

        foreach (var mapping in _iconMappings)
        {
            if (name.Contains(mapping.Key))
                return mapping.Value;
        }

        return "fa-solid fa-box-open"; // Default icon
    }

    /// <summary>
    /// Lấy icon với màu sắc theo loại service
    /// </summary>
    public static (string Icon, string ColorClass) GetIconWithColor(string? serviceName, int? typeId = null)
    {
        var icon = GetIcon(serviceName);
        var color = GetColorByType(typeId);
        return (icon, color);
    }

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
