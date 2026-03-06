using Microsoft.AspNetCore.Http;

namespace HotelBooking.webapp.DTOs.Hotel;

/// <summary>
/// Data Transfer Object for creating a new hotel listing.
/// Handles basic information, selected policies, amenities, and media uploads.
/// </summary>
public class CreateHotelDTO
{
    // --- BASIC INFORMATION ---
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // --- POLICIES ---
    // Individual IDs sent from the Frontend for specific policy categories
    public int SelectedCheckInId { get; set; }
    public int SelectedCheckOutId { get; set; }
    public int SelectedCancellationId { get; set; }

    /// <summary>
    /// Aggregates all selected policy IDs into a single collection for processing.
    /// </summary>
    public List<int> PolicyIds
    {
        get
        {
            var ids = new List<int>();
            if (SelectedCheckInId != 0) ids.Add(SelectedCheckInId);
            if (SelectedCheckOutId != 0) ids.Add(SelectedCheckOutId);
            if (SelectedCancellationId != 0) ids.Add(SelectedCancellationId);
            return ids;
        }
    }

    // --- AMENITIES ---
    // List of selected Amenity IDs provided by the user
    public List<int> AmenityIds { get; set; } = new();

    // --- MEDIA ASSETS ---
    public IFormFile? CoverFile { get; set; }       // Primary banner/thumbnail image
    public IFormFile? MainFile { get; set; }        // Main hero image for the gallery
    public List<IFormFile>? SubFiles { get; set; }  // Supporting gallery images (typically 4 files)
}