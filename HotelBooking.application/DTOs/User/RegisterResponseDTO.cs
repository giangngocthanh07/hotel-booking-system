public class RegisterResponseDTO
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

}