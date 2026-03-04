public interface IFileHelper
{
    // Methods related to file (image) handling
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
            ms.Position = 0; // Important: Reset the stream position

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
            // On error, dispose the stream to avoid memory leaks
            await ms.DisposeAsync();
            throw; // Re-throw to caller
        }

    }
}

