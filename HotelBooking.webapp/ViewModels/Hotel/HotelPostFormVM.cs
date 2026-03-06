using System.ComponentModel.DataAnnotations;

namespace HotelBooking.webapp.ViewModels.Hotel;

/// <summary>
/// ViewModel for submitting a new hotel listing.
/// Aggregates information from all steps: Basic Info, Amenities, Policies, and Media.
/// </summary>
public class HotelPostFormVM
{
    // --- 1. BASIC INFORMATION ---

    [Required(ErrorMessage = "Please enter the hotel name!")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the address!")]
    [MinLength(10, ErrorMessage = "Address must be at least 10 characters long!")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a city!")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid city!")]
    public int CityId { get; set; }

    [MinLength(20, ErrorMessage = "Description must be at least 20 characters long!")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please upload a valid cover image!")]
    public UploadFileVM? CoverFile { get; set; }

    // --- 2. AMENITIES ---

    /// <summary>
    /// Collection of selected Amenity IDs.
    /// </summary>
    public List<int> AmenityIds { get; set; } = new();

    // --- 3. POLICIES ---

    /// <summary>
    /// Collection of selected Policy IDs (Check-in, Check-out, Cancellation, etc.).
    /// </summary>
    public List<int> PolicyIds { get; set; } = new();

    // --- 4. GALLERY IMAGES ---

    [Required(ErrorMessage = "Primary gallery image is required!")]
    public UploadFileVM? MainFile { get; set; }

    /// <summary>
    /// Supporting gallery images (Sub-files).
    /// </summary>
    public List<UploadFileVM> SubFiles { get; set; } = new();
}