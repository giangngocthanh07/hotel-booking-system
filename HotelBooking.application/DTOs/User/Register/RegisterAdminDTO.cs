namespace HotelBooking.application.DTOs.User.Register
{
    /// <summary>
    /// DTO dùng để đăng ký tài khoản khách hàng mới.
    /// Chỉ chứa thông tin cần thiết cho việc tạo tài khoản.
    /// RoleId được mặc định là Customer và không cho phép thay đổi từ client.
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