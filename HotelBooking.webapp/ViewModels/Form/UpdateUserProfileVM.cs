using System.ComponentModel.DataAnnotations;
using HotelBooking.webapp.ViewModels.Validations;

namespace HotelBooking.webapp.ViewModels.Form;

public class UpdateUserProfileVM
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name is too long.")]
    public string FullName { get; set; } = string.Empty;

    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ (10-11 chữ số).")]
    public string? PhoneNumber { get; set; }
    
    [PastDate]
    public DateTime? DateOfBirth { get; set; }
    
}
