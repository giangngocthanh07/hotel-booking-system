public class UploadResultDTO
{
    public Guid? UploadId { get; set; }
    public bool Uploaded { get; set; } = false;
    public string? FileName { get; set; } = string.Empty;
    public string? StoredFileName { get; set; } = string.Empty;
}
