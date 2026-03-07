namespace HotelBooking.webapp.ViewModels.Form;

using System.ComponentModel.DataAnnotations;

public class UserLoginVM
{
    [Required(ErrorMessage = "Please enter your username or email")]
    [MinLength(3, ErrorMessage = "Username/Email must be at least 3 characters")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}