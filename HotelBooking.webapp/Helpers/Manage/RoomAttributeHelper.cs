using HotelBooking.webapp.ViewModels.Hotel;

public static class RoomAttributeHelper
{
    /// <summary>
    /// 1. Factory Pattern: Creates a new CreateVM with default values.
    /// Used for "Add New" actions or resetting the form.
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
                // Note: TypeId will be assigned at the Manager layer
            },

            _ => throw new ArgumentException("Invalid Room Attribute Type")
        };
    }

    /// <summary>
    /// 2. Mapping: Converts Display Data (VM) to Update Data (UpdateVM).
    /// Used when the "Edit" button is clicked on the management table.
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

                // Map dimension parameters
                MinWidth = bed.MinWidth,
                MaxWidth = bed.MaxWidth,

                // Trigger VM setter logic to ensure consistency
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
                // [NOTE]: TypeId is not mapped because changing the type is not allowed during update
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