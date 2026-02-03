using HotelBooking.webapp.ViewModels.Hotel;

public static class RoomAttributeHelper
{
    /// <summary>
    /// 1. Factory Pattern: Tạo CreateVM mới với giá trị mặc định (Dùng cho nút "Thêm mới" hoặc Reset)
    /// </summary>
    public static BaseCreateOrUpdateAdminVM CreateNewCreateModel(RoomAttributeType type)
    {
        return type switch
        {
            RoomAttributeType.UnitType => new UnitTypeCreateVM
            {
                IsEntirePlace = false
            },

            RoomAttributeType.BedType => new BedTypeCreateVM
            {
                DefaultCapacity = 2,
                MinWidth = 38, // Twin
                MaxWidth = 75  // King
            },

            RoomAttributeType.RoomView => new RoomViewCreateVM(),

            RoomAttributeType.RoomQuality => new RoomQualityCreateVM
            {
                SortOrder = 0
                // Lưu ý: TypeId sẽ được gán ở tầng Manager
            },

            _ => throw new ArgumentException("Invalid Room Attribute Type")
        };
    }

    /// <summary>
    /// 2. Mapping: Chuyển từ Dữ liệu hiển thị (VM) sang Dữ liệu Update (UpdateVM)
    /// Dùng khi nhấn nút "Sửa" trên bảng
    /// </summary>
    public static BaseCreateOrUpdateAdminVM? MapToUpdateVM(object source)
    {
        if (source == null) return null;

        return source switch
        {
            // --- Bed Type ---
            BedTypeVM bed => new BedTypeUpdateVM
            {
                Name = bed.Name,
                Description = bed.Description,
                DefaultCapacity = bed.DefaultCapacity,

                // Map thông số kích thước
                MinWidth = bed.MinWidth,
                MaxWidth = bed.MaxWidth,

                // Trigger setter logic của VM để đảm bảo tính nhất quán
                IsVaryingSize = bed.IsVaryingSize
            },

            // --- Unit Type ---
            UnitTypeVM unit => new UnitTypeUpdateVM
            {
                Name = unit.Name,
                Description = unit.Description,
                IsEntirePlace = unit.IsEntirePlace
            },

            // --- Room Quality ---
            RoomQualityVM quality => new RoomQualityUpdateVM
            {
                Name = quality.Name,
                Description = quality.Description,
                SortOrder = quality.SortOrder
                // [LƯU Ý]: Không map TypeId vì Update không cho phép đổi loại
            },

            // --- Room View ---
            RoomViewVM view => new RoomViewUpdateVM
            {
                Name = view.Name,
                Description = view.Description
            },

            _ => null
        };
    }
}