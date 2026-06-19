using System.ComponentModel.DataAnnotations;

namespace HotelBooking.application.DTOs.User;

public class UpdateUserProfileDTO
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }
}
