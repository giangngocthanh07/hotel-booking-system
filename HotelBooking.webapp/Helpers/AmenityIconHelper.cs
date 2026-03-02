namespace HotelBooking.webapp.Helpers;

/// <summary>
/// Helper class để map tên Amenity sang FontAwesome icon.
/// Tách logic ra khỏi component để dễ maintain và test.
/// </summary>
public static class AmenityIconHelper
{
    /// <summary>
    /// Icon mặc định khi không match được keyword nào
    /// </summary>
    public const string DefaultIcon = "fa-solid fa-plus-circle";

    /// <summary>
    /// Dictionary mapping keywords -> icons
    /// Key: lowercase keyword, Value: FontAwesome class
    /// </summary>
    private static readonly Dictionary<string, string> _iconMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Nhóm Vệ sinh / Phòng tắm
        { "towel", "fa-solid fa-pump-soap" },
        { "khăn", "fa-solid fa-pump-soap" },
        { "soap", "fa-solid fa-pump-soap" },
        { "xà bông", "fa-solid fa-pump-soap" },
        { "shampoo", "fa-solid fa-pump-soap" },
        { "dầu gội", "fa-solid fa-pump-soap" },
        { "toilet", "fa-solid fa-toilet" },
        { "vệ sinh", "fa-solid fa-toilet" },
        { "shower", "fa-solid fa-shower" },
        { "vòi sen", "fa-solid fa-shower" },
        { "bathtub", "fa-solid fa-bath" },
        { "bồn tắm", "fa-solid fa-bath" },

        // Nhóm Điện tử / Giải trí
        { "tv", "fa-solid fa-tv" },
        { "tivi", "fa-solid fa-tv" },
        { "television", "fa-solid fa-tv" },
        { "wifi", "fa-solid fa-wifi" },
        { "mạng", "fa-solid fa-wifi" },
        { "internet", "fa-solid fa-wifi" },
        { "phone", "fa-solid fa-phone" },
        { "điện thoại", "fa-solid fa-phone" },

        // Nhóm Đồ gia dụng
        { "fan", "fa-solid fa-fan" },
        { "quạt", "fa-solid fa-fan" },
        { "heat", "fa-solid fa-temperature-arrow-up" },
        { "sưởi", "fa-solid fa-temperature-arrow-up" },
        { "air", "fa-solid fa-snowflake" },
        { "điều hòa", "fa-solid fa-snowflake" },
        { "máy lạnh", "fa-solid fa-snowflake" },
        { "fridge", "fa-solid fa-box" },
        { "tủ lạnh", "fa-solid fa-box" },
        { "minibar", "fa-solid fa-box" },
        { "kettle", "fa-solid fa-mug-hot" },
        { "ấm", "fa-solid fa-mug-hot" },
        { "coffee", "fa-solid fa-mug-hot" },
        { "cà phê", "fa-solid fa-mug-hot" },

        // Nhóm Nội thất
        { "desk", "fa-solid fa-desktop" },
        { "bàn làm việc", "fa-solid fa-desktop" },
        { "chair", "fa-solid fa-chair" },
        { "ghế", "fa-solid fa-chair" },
        { "sofa", "fa-solid fa-couch" },
        { "wardrobe", "fa-solid fa-door-closed" },
        { "tủ quần áo", "fa-solid fa-door-closed" },
        { "safe", "fa-solid fa-lock" },
        { "két sắt", "fa-solid fa-lock" },
        { "mirror", "fa-solid fa-square" },
        { "gương", "fa-solid fa-square" },

        // Nhóm Tiện ích khác
        { "parking", "fa-solid fa-square-parking" },
        { "đỗ xe", "fa-solid fa-square-parking" },
        { "pool", "fa-solid fa-person-swimming" },
        { "hồ bơi", "fa-solid fa-person-swimming" },
        { "gym", "fa-solid fa-dumbbell" },
        { "fitness", "fa-solid fa-dumbbell" },
        { "spa", "fa-solid fa-spa" },
        { "massage", "fa-solid fa-spa" },
        { "breakfast", "fa-solid fa-utensils" },
        { "ăn sáng", "fa-solid fa-utensils" },
        { "restaurant", "fa-solid fa-utensils" },
        { "nhà hàng", "fa-solid fa-utensils" },
        { "laundry", "fa-solid fa-shirt" },
        { "giặt", "fa-solid fa-shirt" },
        { "iron", "fa-solid fa-shirt" },
        { "ủi", "fa-solid fa-shirt" },
        { "elevator", "fa-solid fa-elevator" },
        { "thang máy", "fa-solid fa-elevator" },
        { "wheelchair", "fa-solid fa-wheelchair" },
        { "xe lăn", "fa-solid fa-wheelchair" },
        { "pet", "fa-solid fa-paw" },
        { "thú cưng", "fa-solid fa-paw" },
        { "smoking", "fa-solid fa-smoking" },
        { "hút thuốc", "fa-solid fa-smoking" },
        { "balcony", "fa-solid fa-door-open" },
        { "ban công", "fa-solid fa-door-open" },
    };

    /// <summary>
    /// Lấy icon dựa trên tên amenity.
    /// Tìm keyword trong tên và trả về icon tương ứng.
    /// </summary>
    /// <param name="amenityName">Tên tiện nghi</param>
    /// <returns>FontAwesome icon class</returns>
    public static string GetIcon(string? amenityName)
    {
        if (string.IsNullOrWhiteSpace(amenityName))
            return DefaultIcon;

        var nameLower = amenityName.ToLower();

        // Tìm keyword đầu tiên match
        foreach (var mapping in _iconMappings)
        {
            if (nameLower.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }

        return DefaultIcon;
    }

    /// <summary>
    /// Lấy icon với màu sắc dựa trên type
    /// </summary>
    public static (string Icon, string Color) GetIconWithColor(string? amenityName, string? typeName = null)
    {
        var icon = GetIcon(amenityName);
        var color = GetColorByType(typeName);
        return (icon, color);
    }

    /// <summary>
    /// Lấy màu CSS dựa trên loại amenity
    /// </summary>
    private static string GetColorByType(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return "text-primary";

        var typeNameLower = typeName.ToLower();

        return typeNameLower switch
        {
            var t when t.Contains("bathroom") || t.Contains("phòng tắm") => "text-info",
            var t when t.Contains("bedroom") || t.Contains("phòng ngủ") => "text-warning",
            var t when t.Contains("entertainment") || t.Contains("giải trí") => "text-danger",
            var t when t.Contains("kitchen") || t.Contains("bếp") => "text-success",
            var t when t.Contains("outdoor") || t.Contains("ngoài trời") => "text-secondary",
            _ => "text-primary"
        };
    }
}
