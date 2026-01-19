public class LoginResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
}