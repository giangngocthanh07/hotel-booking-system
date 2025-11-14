public interface IFileHelper
{
    // Các phương thức liên quan đến file (ảnh)
    public Task<UploadFileDTO> ConvertToUploadFileVM(IFormFile file);
};

public class FileHelper : IFileHelper
{
    public async Task<UploadFileDTO> ConvertToUploadFileVM(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var ms = new MemoryStream();
        try
        {
            await file.CopyToAsync(ms);
            ms.Position = 0; // Quan trọng: Reset stream

            var uploadFileDTO = new UploadFileDTO
            {
                FileName = file.FileName,
                Size = file.Length,
                ContentType = file.ContentType,
                Content = ms
            };
            return uploadFileDTO;
        }
        catch (Exception)
        {
            // Nếu có lỗi, hãy tự dọn dẹp stream
            await ms.DisposeAsync();
            throw; // Ném lỗi ra ngoài
        }

    }
}

