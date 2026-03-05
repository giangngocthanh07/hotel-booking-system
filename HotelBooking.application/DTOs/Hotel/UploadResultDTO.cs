public class UploadResultDTO
{
    public bool Uploaded { get; set; } = false;
    public string? FileName { get; set; } = string.Empty;
    public string? StoredFileName { get; set; } = string.Empty;
}