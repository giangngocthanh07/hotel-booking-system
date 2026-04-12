public interface IImageHelper
{
    string GenerateFileName(string prefix, IFormFile file);
    Task<string> UploadAsync(IFormFile file, int userId, int hotelId, string prefix);
    bool Delete(string relativePath);
    string GetImageUrl(string relativePath);
}

public class ImageHelper : IImageHelper
{
    private readonly IWebHostEnvironment _env;

    public ImageHelper(IWebHostEnvironment env)
    {
        _env = env;
    }

    // Generate a normalized file name
    public string GenerateFileName(string prefix, IFormFile file)
    {
        string safeName = Path.GetFileNameWithoutExtension(file.FileName)
                             .Replace(" ", "_")
                             .ToLower();

        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string extension = Path.GetExtension(file.FileName);

        return $"{prefix}_{safeName}_{timestamp}{extension}";
    }

    // Upload image to folder: uploads/userId/{userId}/hotelId/{hotelId}
    public async Task<string> UploadAsync(IFormFile file, int userId, int hotelId, string prefix)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        // Folder path
        string folderPath = Path.Combine(
            _env.WebRootPath,
            "uploads",
            "userId", userId.ToString(),
            "hotelId", hotelId.ToString()
        );

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // File name
        string fileName = GenerateFileName(prefix, file);
        string filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path for DB storage
        string relativePath = Path.Combine("uploads", "userId", userId.ToString(), "hotelId", hotelId.ToString(), fileName)
                              .Replace("\\", "/");

        return relativePath;
    }

    public bool Delete(string relativePath)
    {
        var filePath = Path.Combine(_env.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        return false;
    }

    public string GetImageUrl(string relativePath)
    {
        return string.IsNullOrEmpty(relativePath)
            ? "/images/default.jpg"
            : "/" + relativePath.Replace("\\", "/");
    }
}