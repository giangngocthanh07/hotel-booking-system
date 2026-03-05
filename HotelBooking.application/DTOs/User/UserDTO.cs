namespace HotelBooking.application.DTOs.User;
public class UserDetailDTO
{
    // === Common information ===
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
}