using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace HotelBooking.application.Services.Domains.Media;

public interface IFileService
{
    Task<ApiResponse<UploadResultDTO>> UploadBusinessLicenseAsync(UploadFileDTO file, int userId);
}

public class FileService : IFileService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger _logger;


    public FileService(IConfiguration Configuration, ILogger logger)
    {
        var cloudName = Configuration["Cloudinary:CloudName"];
        var apiKey = Configuration["Cloudinary:ApiKey"];
        var apiSecret = Configuration["Cloudinary:ApiSecret"];


        var acc = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(acc);
        _cloudinary.Api.Secure = true;

        _logger = logger;
    }

    public async Task<ApiResponse<UploadResultDTO>> UploadBusinessLicenseAsync(UploadFileDTO file, int userId)
    {
        try
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isImage = extension == ".jpg" || extension == ".jpeg" || extension == ".png";

            UploadResultDTO result = new UploadResultDTO();

            if (isImage)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.Content),
                    Transformation = new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto"),
                    PublicId = $"user_{userId}_business_license_{Guid.NewGuid()}",
                    Folder = $"HotelBooking/Approvals/Pendings/user_{userId}/business_license"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    result.Uploaded = false;
                    result.FileName = file.FileName;
                    result.StoredFileName = null;

                    return ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, MessageResponse.Common.UPDATE_FAILED);
                }
                else
                {
                    result.Uploaded = true;
                    result.FileName = file.FileName;
                    result.StoredFileName = uploadResult.SecureUrl.ToString();

                    return ResponseFactory.Success(result, MessageResponse.Common.UPDATE_SUCCESSFULLY);
                }
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, file.Content),
                    PublicId = $"user_{userId}_business_license_{Guid.NewGuid()}",
                    Folder = $"HotelBooking/Approvals/Pendings/user_{userId}/business_license"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    result.FileName = file.FileName;
                    result.Uploaded = false;
                    result.StoredFileName = null;

                    return ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, MessageResponse.Common.UPDATE_FAILED);
                }
                else
                {
                    result.FileName = file.FileName;
                    result.Uploaded = true;
                    result.StoredFileName = uploadResult.SecureUrl.ToString();

                    return ResponseFactory.Success(result, MessageResponse.Common.UPDATE_SUCCESSFULLY);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("FileService.UploadBusinessLicenseAsync: {ErrorMessage}", ex.Message);
            return ResponseFactory.ServerError<UploadResultDTO>();
        }
    }
}
