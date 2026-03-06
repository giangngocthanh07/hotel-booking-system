using System.ComponentModel.DataAnnotations;
using HotelBooking.webapp.ViewModels.Admin.Base;

namespace HotelBooking.webapp.ViewModels.Admin;

/// <summary>
/// ViewModel for displaying Amenity Types (e.g., Room Amenities, Hotel Services).
/// </summary>
public class AmenityTypeVM : BaseAdminVM
{
    public string? IconClass { get; set; }
    public string? IconColor { get; set; }
}

/// <summary>
/// ViewModel for displaying specific Amenities.
/// </summary>
public class AmenityVM : BaseAdminVM
{
    public int TypeId { get; set; }
}

/// <summary>
/// ViewModel for creating a new Amenity.
/// Requires TypeId to associate with a category.
/// </summary>
public class AmenityCreateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Amenity Type is required!")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Amenity Type")]
    public int TypeId { get; set; }
}

/// <summary>
/// ViewModel for updating an existing Amenity.
/// Inherits Name and Description from base; TypeId is excluded to prevent accidental category changes.
/// </summary>
public class AmenityUpdateVM : BaseCreateOrUpdateAdminVM
{
    // Inherits Name and Description from BaseCreateOrUpdateAdminVM
}