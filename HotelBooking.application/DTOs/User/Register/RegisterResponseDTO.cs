namespace HotelBooking.application.DTOs.User.Register
{
    /// <summary>
    /// DTO dùng để trả về kết quả sau khi đăng ký tài khoản.
    /// Chỉ chứa thông tin cần thiết để xác nhận việc tạo tài khoản đã thành công hay chưa.
    /// </summary>

    public class RegisterResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

    }
}