using HotelBooking.application.DTOs.Role;

namespace HotelBooking.application.DTOs.User.Register
{
    /// <summary>
    /// DTO used to register a new admin user.
    /// Contains only the necessary information for creating an account.
    /// RoleId is set to Admin by default and cannot be changed from client.
    /// </summary>

    public class RegisterAdminDTO
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        private int RoleId = RoleTypeConstDTO.Admin;
        public int GetRoleId()
        {
            return RoleId;
        }
    }
}