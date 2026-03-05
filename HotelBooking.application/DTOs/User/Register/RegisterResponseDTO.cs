namespace HotelBooking.application.DTOs.User.Register
{
    /// <summary>
    /// DTO used to return the result after registering an account.
    /// Contains only the necessary information to confirm whether the account creation was successful or not.
    /// </summary>

    public class RegisterResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

    }
}