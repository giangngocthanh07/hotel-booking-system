public class UploadFileDTO
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; } = 0;
    public Stream Content { get; set; } = null!;
}