using System.ComponentModel.DataAnnotations;

namespace HotelBooking.webapp.ViewModels.Admin.Base;

/// <summary>
/// Base class for Admin Data Transfer Objects / ViewModels used in listing and details.
/// </summary>
public abstract class BaseAdminVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required!")]
    [MaxLength(500, ErrorMessage = "Name is too long (maximum 500 characters)!")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description is too long (maximum 500 characters)!")]
    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }
}

/// <summary>
/// Base class for Create and Update operations.
/// </summary>
public abstract class BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Name is required!")]
    [MaxLength(50, ErrorMessage = "Name is too long (maximum 50 characters)!")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description is too long (maximum 500 characters)!")]
    public string? Description { get; set; }
}