namespace HotelBooking.application.DTOs.User.Login
{
    /// <summary>
    /// DTO dùng để đăng nhập tài khoản.
    /// Chứa thông tin cần thiết để xác thực người dùng.
    /// </summary>
    public class LoginUserDTO
    {
        public string UsernameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}