
public class UploadFileVM
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long Size { get; set; }
    public Stream Content { get; set; }
}