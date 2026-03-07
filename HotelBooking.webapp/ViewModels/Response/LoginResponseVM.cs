namespace HotelBooking.webapp.ViewModels.Response;

public class LoginResponseVM
{
    public string AccessToken { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}