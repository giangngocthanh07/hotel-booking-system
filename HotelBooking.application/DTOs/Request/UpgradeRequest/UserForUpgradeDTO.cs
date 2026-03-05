namespace HotelBooking.application.DTOs.Request.UpgradeRequest;
public class UserForUpgradeDTO
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? RequestStatus { get; set; } = "None";
}