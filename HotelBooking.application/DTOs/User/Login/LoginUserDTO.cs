namespace HotelBooking.application.DTOs.User.Login
{
    /// <summary>
    /// DTO used to login an account.
    /// Contains only the necessary information for authentication.
    /// </summary>
    public class LoginUserDTO
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}