public class UploadFileDTO
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public Stream Content { get; set; }
}